#!/usr/bin/env bash

set -euo pipefail

readonly MODE=${1:-}
readonly QEMU_EXECUTABLE=${S390X_QEMU_EXECUTABLE:-/usr/bin/qemu-system-s390x}
VM_WORK_DIRECTORY=

atomic_write()
{
    local path=$1
    local value=$2
    local temporary_path="${path}.tmp.$$"

    printf '%s\n' "$value" > "$temporary_path"
    mv -f "$temporary_path" "$path"
}

require_command()
{
    command -v "$1" > /dev/null || { printf 'Required command not found: %s\n' "$1" >&2; return 1; }
}

unit_load_state()
{
    sudo systemctl show --property=LoadState --value "$1" 2> /dev/null || true
}

unit_is_active()
{
    local state
    state=$(sudo systemctl is-active "$1" 2> /dev/null || true)
    [[ "$state" == active || "$state" == activating || "$state" == deactivating ]]
}

stop_unit()
{
    local unit=$1
    local load_state
    load_state=$(unit_load_state "$unit")
    if [[ -z "$load_state" || "$load_state" == not-found ]]; then
        return 0
    fi

    sudo systemctl stop "$unit" > /dev/null 2>&1 || true
    for _ in {1..30}; do
        if ! unit_is_active "$unit"; then
            sudo systemctl reset-failed "$unit" > /dev/null 2>&1 || true
            return 0
        fi
        sleep 1
    done

    sudo systemctl kill --kill-who=all --signal=SIGKILL "$unit" > /dev/null 2>&1 || true
    for _ in {1..10}; do
        if ! unit_is_active "$unit"; then
            sudo systemctl reset-failed "$unit" > /dev/null 2>&1 || true
            return 0
        fi
        sleep 1
    done

    printf 'Failed to stop systemd unit %s\n' "$unit" >&2
    return 1
}

start_service()
{
    local unit=$1
    local runtime=$2
    local working_directory=$3
    local output_path=$4
    shift 4

    local -a output_properties=()
    if [[ -n "$output_path" ]]; then
        output_properties+=(
            "--property=StandardOutput=append:$output_path"
            "--property=StandardError=append:$output_path"
        )
    fi

    sudo systemd-run \
        --quiet \
        --unit="$unit" \
        --service-type=exec \
        --collect \
        --uid="$(id -u)" \
        --gid="$(id -g)" \
        --working-directory="$working_directory" \
        --property="RuntimeMaxSec=$runtime" \
        --property=TimeoutStopSec=30s \
        --property=KillMode=control-group \
        --property=NoNewPrivileges=yes \
        "${output_properties[@]}" \
        -- "$@"
}

wait_for_inactive()
{
    local unit=$1
    local seconds=$2

    for ((i = 0; i < seconds; ++i)); do
        if ! unit_is_active "$unit"; then
            return 0
        fi
        sleep 1
    done
    return 1
}

remove_work_directory()
{
    if [[ -n "$VM_WORK_DIRECTORY" ]]; then
        rm -rf "$VM_WORK_DIRECTORY"
        VM_WORK_DIRECTORY=
    fi
}

run_exit_cleanup()
{
    cleanup || true
    remove_work_directory
}

self_test()
{
    require_command sudo
    require_command systemctl
    require_command systemd-run

    local prefix="linuxcore-s390x-self-test-$$-$RANDOM"
    SELF_TEST_CLEANUP_UNIT="${prefix}-cleanup.service"
    SELF_TEST_RUNTIME_UNIT="${prefix}-runtime.service"
    trap 'stop_unit "$SELF_TEST_CLEANUP_UNIT" || true; stop_unit "$SELF_TEST_RUNTIME_UNIT" || true' EXIT

    start_service "$SELF_TEST_CLEANUP_UNIT" 60s /tmp '' /usr/bin/sleep 300
    unit_is_active "$SELF_TEST_CLEANUP_UNIT"
    stop_unit "$SELF_TEST_CLEANUP_UNIT"
    if unit_is_active "$SELF_TEST_CLEANUP_UNIT"; then
        return 1
    fi

    start_service "$SELF_TEST_RUNTIME_UNIT" 2s /tmp '' /usr/bin/sleep 300
    unit_is_active "$SELF_TEST_RUNTIME_UNIT"
    wait_for_inactive "$SELF_TEST_RUNTIME_UNIT" 15

    stop_unit "${prefix}-missing.service"

    VM_WORK_DIRECTORY=$(mktemp -d "${RUNNER_TEMP:-/tmp}/linuxcore-s390x-trap-test.XXXXXX")
    local trap_test_directory=$VM_WORK_DIRECTORY
    (trap remove_work_directory EXIT; true)
    if [[ -e "$trap_test_directory" ]]; then
        printf 'Success-exit cleanup did not remove %s\n' "$trap_test_directory" >&2
        return 1
    fi
    VM_WORK_DIRECTORY=

    printf 'systemd VM cleanup self-test passed\n'
}

initialize_paths()
{
    : "${S390X_ARTIFACT_DIRECTORY:?S390X_ARTIFACT_DIRECTORY is required}"
    : "${S390X_STATE_DIRECTORY:?S390X_STATE_DIRECTORY is required}"
    : "${S390X_UNIT_NAME:?S390X_UNIT_NAME is required}"

    [[ "$S390X_UNIT_NAME" =~ ^linuxcore-s390x-[A-Za-z0-9_.@-]+\.service$ ]] || {
        printf 'Invalid systemd unit name: %s\n' "$S390X_UNIT_NAME" >&2
        return 1
    }

    mkdir -p "$S390X_ARTIFACT_DIRECTORY" "$S390X_STATE_DIRECTORY"
    STATE_FILE="$S390X_STATE_DIRECTORY/unit"
    HELPER_STATUS_FILE="$S390X_ARTIFACT_DIRECTORY/helper.status"
    CLEANUP_STATUS_FILE="$S390X_ARTIFACT_DIRECTORY/cleanup.status"
}

cleanup()
{
    initialize_paths
    rm -f "$CLEANUP_STATUS_FILE"

    if [[ -f "$STATE_FILE" ]]; then
        local recorded_unit
        recorded_unit=$(<"$STATE_FILE")
        if [[ "$recorded_unit" != "$S390X_UNIT_NAME" ]]; then
            printf 'Recorded unit %s does not match expected unit %s\n' "$recorded_unit" "$S390X_UNIT_NAME" >&2
            return 1
        fi
        stop_unit "$recorded_unit"
        rm -f "$STATE_FILE"
    else
        stop_unit "$S390X_UNIT_NAME"
    fi

    atomic_write "$CLEANUP_STATUS_FILE" success
}

remaining_seconds()
{
    local maximum=$1
    local remaining=$((GLOBAL_DEADLINE - SECONDS))
    if ((remaining <= 0)); then
        return 1
    fi
    if ((maximum < remaining)); then
        printf '%s\n' "$maximum"
    else
        printf '%s\n' "$remaining"
    fi
}

run_bounded()
{
    local maximum=$1
    shift
    local seconds
    seconds=$(remaining_seconds "$maximum") || return 124
    timeout --signal=TERM --kill-after=10s "${seconds}s" "$@"
}

run_vm()
{
    initialize_paths
    : "${S390X_IMAGE:?S390X_IMAGE is required}"
    : "${S390X_RUNNER_ARCHIVE:?S390X_RUNNER_ARCHIVE is required}"

    [[ "$QEMU_EXECUTABLE" == /* ]] || { printf 'QEMU executable must be absolute\n' >&2; return 1; }
    [[ -f "$S390X_IMAGE" ]] || { printf 'Guest image not found: %s\n' "$S390X_IMAGE" >&2; return 1; }
    [[ -f "$S390X_RUNNER_ARCHIVE" ]] || { printf 'Test runner archive not found: %s\n' "$S390X_RUNNER_ARCHIVE" >&2; return 1; }

    require_command cloud-localds
    require_command qemu-img
    require_command ssh
    require_command scp
    require_command ssh-keygen
    require_command systemctl
    require_command systemd-run
    require_command timeout
    [[ -x "$QEMU_EXECUTABLE" ]] || { printf 'QEMU executable is not executable: %s\n' "$QEMU_EXECUTABLE" >&2; return 1; }

    readonly GLOBAL_DEADLINE=$((SECONDS + 78 * 60))
    VM_WORK_DIRECTORY=$(mktemp -d "${RUNNER_TEMP:-/tmp}/linuxcore-s390x-vm.XXXXXX")
    trap run_exit_cleanup EXIT
    trap 'exit 129' HUP
    trap 'exit 130' INT
    trap 'exit 143' TERM

    rm -f "$HELPER_STATUS_FILE" "$CLEANUP_STATUS_FILE"
    cleanup
    rm -f "$CLEANUP_STATUS_FILE"

    local ssh_key="$VM_WORK_DIRECTORY/id_ed25519"
    local public_key
    ssh-keygen -q -t ed25519 -N '' -f "$ssh_key"
    public_key=$(<"${ssh_key}.pub")

    local user_data="$VM_WORK_DIRECTORY/user-data"
    local meta_data="$VM_WORK_DIRECTORY/meta-data"
    cat > "$user_data" <<EOF
#cloud-config
users:
  - default
  - name: linuxcore
    groups: [wheel]
    shell: /bin/bash
    lock_passwd: true
    sudo: ["ALL=(ALL) NOPASSWD:ALL"]
    ssh_authorized_keys:
      - $public_key
ssh_pwauth: false
disable_root: true
runcmd:
  - [systemctl, enable, --now, sshd.service]
EOF
    cat > "$meta_data" <<EOF
instance-id: ${S390X_UNIT_NAME%.service}
local-hostname: linuxcore-s390x
EOF

    local seed="$VM_WORK_DIRECTORY/seed.img"
    local overlay="$VM_WORK_DIRECTORY/guest.qcow2"
    cloud-localds "$seed" "$user_data" "$meta_data"
    qemu-img create -q -f qcow2 -F qcow2 -b "$S390X_IMAGE" "$overlay"
    qemu-img resize -q "$overlay" 8G

    local ssh_port
    ssh_port=$(python3 - <<'PY'
import socket
with socket.socket() as sock:
    sock.bind(("127.0.0.1", 0))
    print(sock.getsockname()[1])
PY
)

    atomic_write "$STATE_FILE" "$S390X_UNIT_NAME"
    start_service \
        "$S390X_UNIT_NAME" \
        80min \
        "$VM_WORK_DIRECTORY" \
        "$S390X_ARTIFACT_DIRECTORY/systemd.log" \
        /usr/bin/env -i PATH=/usr/bin:/bin \
        "$QEMU_EXECUTABLE" \
        -machine s390-ccw-virtio,accel=tcg \
        -cpu max \
        -smp 2 \
        -m 4096 \
        -drive "if=none,id=root,file=$overlay,format=qcow2" \
        -device virtio-blk-ccw,drive=root,bootindex=1 \
        -drive "if=none,id=seed,file=$seed,format=raw,readonly=on" \
        -device virtio-blk-ccw,drive=seed \
        -netdev "user,id=net0,hostfwd=tcp:127.0.0.1:$ssh_port-:22" \
        -device virtio-net-ccw,netdev=net0 \
        -device virtio-rng-ccw \
        -display none \
        -monitor none \
        -serial "file:$S390X_ARTIFACT_DIRECTORY/serial.log" \
        -no-reboot

    local -a common_ssh_options=(
        -i "$ssh_key"
        -o "BatchMode=yes"
        -o "ConnectTimeout=5"
        -o "IdentitiesOnly=yes"
        -o "StrictHostKeyChecking=no"
        -o "UserKnownHostsFile=/dev/null"
    )
    local -a ssh_options=(-p "$ssh_port" "${common_ssh_options[@]}")
    local -a scp_options=(-P "$ssh_port" "${common_ssh_options[@]}")

    local ssh_ready=false
    local ssh_deadline=$((SECONDS + 10 * 60))
    while ((SECONDS < ssh_deadline && SECONDS < GLOBAL_DEADLINE)); do
        if ! unit_is_active "$S390X_UNIT_NAME"; then
            printf 'QEMU exited before SSH became ready\n' >&2
            return 1
        fi
        if ssh "${ssh_options[@]}" linuxcore@127.0.0.1 true > /dev/null 2>&1; then
            ssh_ready=true
            break
        fi
        sleep 5
    done
    if [[ "$ssh_ready" != true ]]; then
        printf 'SSH did not become ready; cloud-init status is unavailable\n' | tee "$S390X_ARTIFACT_DIRECTORY/cloud-init-unavailable.log" >&2
        return 1
    fi

    set +e
    run_bounded $((8 * 60)) ssh "${ssh_options[@]}" linuxcore@127.0.0.1 'sudo cloud-init status --wait' \
        2>&1 | tee "$S390X_ARTIFACT_DIRECTORY/cloud-init-wait.log"
    local cloud_init_status=${PIPESTATUS[0]}
    run_bounded 30 ssh "${ssh_options[@]}" linuxcore@127.0.0.1 'sudo cloud-init status --format json' \
        > "$S390X_ARTIFACT_DIRECTORY/cloud-init-status.json" 2>&1
    local cloud_init_json_status=$?
    set -e
    if ((cloud_init_status != 0 || cloud_init_json_status != 0)); then
        return 1
    fi

    set +e
    run_bounded $((12 * 60)) ssh "${ssh_options[@]}" linuxcore@127.0.0.1 \
        'sudo env LC_ALL=C.UTF-8 dnf install -y dotnet-runtime-10.0 gcc glibc-devel kernel-headers' \
        2>&1 | tee "$S390X_ARTIFACT_DIRECTORY/dnf-install.log"
    local dnf_status=${PIPESTATUS[0]}
    set -e
    if ((dnf_status != 0)); then
        return "$dnf_status"
    fi

    run_bounded 120 ssh "${ssh_options[@]}" linuxcore@127.0.0.1 \
        'dotnet --info; printf "\nInstalled packages:\n"; rpm -q dotnet-runtime-10.0 gcc glibc-devel kernel-headers; printf "\nDNF transaction:\n"; sudo dnf history info last' \
        > "$S390X_ARTIFACT_DIRECTORY/guest-versions.log" 2>&1

    run_bounded $((7 * 60)) scp "${scp_options[@]}" "$S390X_RUNNER_ARCHIVE" linuxcore@127.0.0.1:/tmp/LinuxCore.Tests.tar.gz
    run_bounded 60 ssh "${ssh_options[@]}" linuxcore@127.0.0.1 \
        'rm -rf /tmp/linuxcore-tests && mkdir /tmp/linuxcore-tests && tar -xzf /tmp/LinuxCore.Tests.tar.gz -C /tmp/linuxcore-tests'

    set +e
    run_bounded $((35 * 60)) ssh "${ssh_options[@]}" linuxcore@127.0.0.1 \
        'cd /tmp/linuxcore-tests && env LC_ALL=C.UTF-8 DOTNET_CLI_TELEMETRY_OPTOUT=1 LINUXCORE_EXPECTED_ARCHITECTURE=s390x LINUXCORE_EXPECTED_QEMU_LINUX_USER=false LINUXCORE_EXPECTED_LIBC_IMPLEMENTATION=glibc dotnet LinuxCore.Tests.dll --no-banner --no-ansi --progress off' \
        2>&1 | tee "$S390X_ARTIFACT_DIRECTORY/test-output.log"
    local test_status=${PIPESTATUS[0]}
    set -e
    atomic_write "$S390X_ARTIFACT_DIRECTORY/test.exitcode" "$test_status"

    set +e
    run_bounded 60 ssh "${ssh_options[@]}" linuxcore@127.0.0.1 'sudo systemctl poweroff' > /dev/null 2>&1
    set -e
    if ! wait_for_inactive "$S390X_UNIT_NAME" $((5 * 60)); then
        printf 'Guest did not power off cleanly\n' >&2
        return 1
    fi

    cleanup
    if ((test_status != 0)); then
        return "$test_status"
    fi
    atomic_write "$HELPER_STATUS_FILE" success
}

case "$MODE" in
    run)
        run_vm
        ;;
    cleanup)
        cleanup
        ;;
    self-test)
        self_test
        ;;
    *)
        printf 'Usage: %s {run|cleanup|self-test}\n' "$0" >&2
        exit 2
        ;;
esac
