using System.Diagnostics.CodeAnalysis;

namespace LinuxCore;

[SuppressMessage("Microsoft.Formatting", "IDE0055: Fix formatting", Justification = "Intentional enum value alignment")]
[SuppressMessage("Design", "CA1069: Enums values should not be duplicated", Justification = "Mirrors glibc _SC_IOV_MAX = _SC_UIO_MAXIOV alias")]
public enum SysConfName
{
    ArgMax                      = 0,   // _SC_ARG_MAX
    ChildMax                    = 1,   // _SC_CHILD_MAX
    ClkTck                      = 2,   // _SC_CLK_TCK
    NGroupsMax                  = 3,   // _SC_NGROUPS_MAX
    OpenMax                     = 4,   // _SC_OPEN_MAX
    StreamMax                   = 5,   // _SC_STREAM_MAX
    TzNameMax                   = 6,   // _SC_TZNAME_MAX
    JobControl                  = 7,   // _SC_JOB_CONTROL
    SavedIds                    = 8,   // _SC_SAVED_IDS
    RealTimeSignals             = 9,   // _SC_REALTIME_SIGNALS
    PriorityScheduling          = 10,  // _SC_PRIORITY_SCHEDULING
    Timers                      = 11,  // _SC_TIMERS
    AsynchronousIO              = 12,  // _SC_ASYNCHRONOUS_IO
    PrioritizedIO               = 13,  // _SC_PRIORITIZED_IO
    SynchronizedIO              = 14,  // _SC_SYNCHRONIZED_IO
    FSync                       = 15,  // _SC_FSYNC
    MappedFiles                 = 16,  // _SC_MAPPED_FILES
    MemLock                     = 17,  // _SC_MEMLOCK
    MemLockRange                = 18,  // _SC_MEMLOCK_RANGE
    MemoryProtection            = 19,  // _SC_MEMORY_PROTECTION
    MessagePassing              = 20,  // _SC_MESSAGE_PASSING
    Semaphores                  = 21,  // _SC_SEMAPHORES
    SharedMemoryObjects         = 22,  // _SC_SHARED_MEMORY_OBJECTS
    AioListioMax                = 23,  // _SC_AIO_LISTIO_MAX
    AioMax                      = 24,  // _SC_AIO_MAX
    AioPrioDeltaMax             = 25,  // _SC_AIO_PRIO_DELTA_MAX
    DelaytimerMax               = 26,  // _SC_DELAYTIMER_MAX
    MqOpenMax                   = 27,  // _SC_MQ_OPEN_MAX
    MqPrioMax                   = 28,  // _SC_MQ_PRIO_MAX
    Version                     = 29,  // _SC_VERSION
    PageSize                    = 30,  // _SC_PAGESIZE
    RtSigMax                    = 31,  // _SC_RTSIG_MAX
    SemNsemsMax                 = 32,  // _SC_SEM_NSEMS_MAX
    SemValueMax                 = 33,  // _SC_SEM_VALUE_MAX
    SigQueueMax                 = 34,  // _SC_SIGQUEUE_MAX
    TimerMax                    = 35,  // _SC_TIMER_MAX

    // Values corresponding to _POSIX2_* symbols
    BcBaseMax                   = 36,  // _SC_BC_BASE_MAX
    BcDimMax                    = 37,  // _SC_BC_DIM_MAX
    BcScaleMax                  = 38,  // _SC_BC_SCALE_MAX
    BcStringMax                 = 39,  // _SC_BC_STRING_MAX
    CollWeightsMax              = 40,  // _SC_COLL_WEIGHTS_MAX
    EquivClassMax               = 41,  // _SC_EQUIV_CLASS_MAX
    ExprNestMax                 = 42,  // _SC_EXPR_NEST_MAX
    LineMax                     = 43,  // _SC_LINE_MAX
    ReDupMax                    = 44,  // _SC_RE_DUP_MAX
    CharClassNameMax            = 45,  // _SC_CHARCLASS_NAME_MAX

    Posix2Version               = 46,  // _SC_2_VERSION
    Posix2CBind                 = 47,  // _SC_2_C_BIND
    Posix2CDev                  = 48,  // _SC_2_C_DEV
    Posix2FortDev               = 49,  // _SC_2_FORT_DEV
    Posix2FortRun               = 50,  // _SC_2_FORT_RUN
    Posix2SwDev                 = 51,  // _SC_2_SW_DEV
    Posix2Localedef             = 52,  // _SC_2_LOCALEDEF

    Pii                         = 53,  // _SC_PII
    PiiXti                      = 54,  // _SC_PII_XTI
    PiiSocket                   = 55,  // _SC_PII_SOCKET
    PiiInternet                 = 56,  // _SC_PII_INTERNET
    PiiOsi                      = 57,  // _SC_PII_OSI
    Poll                        = 58,  // _SC_POLL
    Select                      = 59,  // _SC_SELECT
    UioMaxiov                   = 60,  // _SC_UIO_MAXIOV
    IovMax                      = 60,  // _SC_IOV_MAX (= _SC_UIO_MAXIOV)
    PiiInternetStream           = 61,  // _SC_PII_INTERNET_STREAM
    PiiInternetDgram            = 62,  // _SC_PII_INTERNET_DGRAM
    PiiOsiCots                  = 63,  // _SC_PII_OSI_COTS
    PiiOsiClts                  = 64,  // _SC_PII_OSI_CLTS
    PiiOsiM                     = 65,  // _SC_PII_OSI_M
    TIovMax                     = 66,  // _SC_T_IOV_MAX

    // POSIX 1003.1c (POSIX threads)
    Threads                     = 67,  // _SC_THREADS
    ThreadSafeFunctions         = 68,  // _SC_THREAD_SAFE_FUNCTIONS
    GetGrRSizeMax               = 69,  // _SC_GETGR_R_SIZE_MAX
    GetPwRSizeMax               = 70,  // _SC_GETPW_R_SIZE_MAX
    LoginNameMax                = 71,  // _SC_LOGIN_NAME_MAX
    TtyNameMax                  = 72,  // _SC_TTY_NAME_MAX
    ThreadDestructorIterations  = 73,  // _SC_THREAD_DESTRUCTOR_ITERATIONS
    ThreadKeysMax               = 74,  // _SC_THREAD_KEYS_MAX
    ThreadStackMin              = 75,  // _SC_THREAD_STACK_MIN
    ThreadThreadsMax            = 76,  // _SC_THREAD_THREADS_MAX
    ThreadAttrStackaddr         = 77,  // _SC_THREAD_ATTR_STACKADDR
    ThreadAttrStacksize         = 78,  // _SC_THREAD_ATTR_STACKSIZE
    ThreadPriorityScheduling    = 79,  // _SC_THREAD_PRIORITY_SCHEDULING
    ThreadPrioInherit           = 80,  // _SC_THREAD_PRIO_INHERIT
    ThreadPrioProtect           = 81,  // _SC_THREAD_PRIO_PROTECT
    ThreadProcessShared         = 82,  // _SC_THREAD_PROCESS_SHARED

    NprocessorsConf             = 83,  // _SC_NPROCESSORS_CONF
    NprocessorsOnln             = 84,  // _SC_NPROCESSORS_ONLN
    PhysPages                   = 85,  // _SC_PHYS_PAGES
    AvphysPages                 = 86,  // _SC_AVPHYS_PAGES
    AtexitMax                   = 87,  // _SC_ATEXIT_MAX
    PassMax                     = 88,  // _SC_PASS_MAX

    XOpenVersion                = 89,  // _SC_XOPEN_VERSION
    XOpenXcuVersion             = 90,  // _SC_XOPEN_XCU_VERSION
    XOpenUnix                   = 91,  // _SC_XOPEN_UNIX
    XOpenCrypt                  = 92,  // _SC_XOPEN_CRYPT
    XOpenEnhI18n                = 93,  // _SC_XOPEN_ENH_I18N
    XOpenShm                    = 94,  // _SC_XOPEN_SHM

    Posix2CharTerm              = 95,  // _SC_2_CHAR_TERM
    Posix2CVersion              = 96,  // _SC_2_C_VERSION
    Posix2Upe                   = 97,  // _SC_2_UPE

    XopenXpg2                   = 98,  // _SC_XOPEN_XPG2
    XopenXpg3                   = 99,  // _SC_XOPEN_XPG3
    XopenXpg4                   = 100, // _SC_XOPEN_XPG4

    CharBit                     = 101, // _SC_CHAR_BIT
    CharMax                     = 102, // _SC_CHAR_MAX
    CharMin                     = 103, // _SC_CHAR_MIN
    IntMax                      = 104, // _SC_INT_MAX
    IntMin                      = 105, // _SC_INT_MIN
    LongBit                     = 106, // _SC_LONG_BIT
    WordBit                     = 107, // _SC_WORD_BIT
    MbLenMax                    = 108, // _SC_MB_LEN_MAX
    Nzero                       = 109, // _SC_NZERO
    SSizeMax                    = 110, // _SC_SSIZE_MAX
    SCharMax                    = 111, // _SC_SCHAR_MAX
    SCharMin                    = 112, // _SC_SCHAR_MIN
    ShrtMax                     = 113, // _SC_SHRT_MAX
    ShrtMin                     = 114, // _SC_SHRT_MIN
    UCharMax                    = 115, // _SC_UCHAR_MAX
    UIntMax                     = 116, // _SC_UINT_MAX
    ULongMax                    = 117, // _SC_ULONG_MAX
    UShrtMax                    = 118, // _SC_USHRT_MAX

    NlArgmax                    = 119, // _SC_NL_ARGMAX
    NlLangmax                   = 120, // _SC_NL_LANGMAX
    NlMsgmax                    = 121, // _SC_NL_MSGMAX
    NlNmax                      = 122, // _SC_NL_NMAX
    NlSetmax                    = 123, // _SC_NL_SETMAX
    NlTextmax                   = 124, // _SC_NL_TEXTMAX

    Xbs5Ilp32Off32              = 125, // _SC_XBS5_ILP32_OFF32
    Xbs5Ilp32Offbig             = 126, // _SC_XBS5_ILP32_OFFBIG
    Xbs5Lp64Off64               = 127, // _SC_XBS5_LP64_OFF64
    Xbs5LpbigOffbig             = 128, // _SC_XBS5_LPBIG_OFFBIG

    XopenLegacy                 = 129, // _SC_XOPEN_LEGACY
    XopenRealtime               = 130, // _SC_XOPEN_REALTIME
    XopenRealtimeThreads        = 131, // _SC_XOPEN_REALTIME_THREADS

    AdvisoryInfo                = 132, // _SC_ADVISORY_INFO
    Barriers                    = 133, // _SC_BARRIERS
    Base                        = 134, // _SC_BASE
    CLangSupport                = 135, // _SC_C_LANG_SUPPORT
    CLangSupportR               = 136, // _SC_C_LANG_SUPPORT_R
    ClockSelection              = 137, // _SC_CLOCK_SELECTION
    Cputime                     = 138, // _SC_CPUTIME
    ThreadCputime               = 139, // _SC_THREAD_CPUTIME
    DeviceIO                    = 140, // _SC_DEVICE_IO
    DeviceSpecific              = 141, // _SC_DEVICE_SPECIFIC
    DeviceSpecificR             = 142, // _SC_DEVICE_SPECIFIC_R
    FdMgmt                      = 143, // _SC_FD_MGMT
    Fifo                        = 144, // _SC_FIFO
    Pipe                        = 145, // _SC_PIPE
    FileAttributes              = 146, // _SC_FILE_ATTRIBUTES
    FileLocking                 = 147, // _SC_FILE_LOCKING
    FileSystem                  = 148, // _SC_FILE_SYSTEM
    MonotonicClock              = 149, // _SC_MONOTONIC_CLOCK
    MultiProcess                = 150, // _SC_MULTI_PROCESS
    SingleProcess               = 151, // _SC_SINGLE_PROCESS
    Networking                  = 152, // _SC_NETWORKING
    ReaderWriterLocks           = 153, // _SC_READER_WRITER_LOCKS
    SpinLocks                   = 154, // _SC_SPIN_LOCKS
    Regexp                      = 155, // _SC_REGEXP
    RegexVersion                = 156, // _SC_REGEX_VERSION
    Shell                       = 157, // _SC_SHELL
    Signals                     = 158, // _SC_SIGNALS
    Spawn                       = 159, // _SC_SPAWN
    SporadicServer              = 160, // _SC_SPORADIC_SERVER
    ThreadSporadicServer        = 161, // _SC_THREAD_SPORADIC_SERVER
    SystemDatabase              = 162, // _SC_SYSTEM_DATABASE
    SystemDatabaseR             = 163, // _SC_SYSTEM_DATABASE_R
    Timeouts                    = 164, // _SC_TIMEOUTS
    TypedMemoryObjects          = 165, // _SC_TYPED_MEMORY_OBJECTS
    UserGroups                  = 166, // _SC_USER_GROUPS
    UserGroupsR                 = 167, // _SC_USER_GROUPS_R
    Posix2Pbs                   = 168, // _SC_2_PBS
    Posix2PbsAccounting         = 169, // _SC_2_PBS_ACCOUNTING
    Posix2PbsLocate             = 170, // _SC_2_PBS_LOCATE
    Posix2PbsMessage            = 171, // _SC_2_PBS_MESSAGE
    Posix2PbsTrack              = 172, // _SC_2_PBS_TRACK
    SymloopMax                  = 173, // _SC_SYMLOOP_MAX
    Streams                     = 174, // _SC_STREAMS
    Posix2PbsCheckpoint         = 175, // _SC_2_PBS_CHECKPOINT

    V6Ilp32Off32                = 176, // _SC_V6_ILP32_OFF32
    V6Ilp32Offbig               = 177, // _SC_V6_ILP32_OFFBIG
    V6Lp64Off64                 = 178, // _SC_V6_LP64_OFF64
    V6LpbigOffbig               = 179, // _SC_V6_LPBIG_OFFBIG

    HostNameMax                 = 180, // _SC_HOST_NAME_MAX
    Trace                       = 181, // _SC_TRACE
    TraceEventFilter            = 182, // _SC_TRACE_EVENT_FILTER
    TraceInherit                = 183, // _SC_TRACE_INHERIT
    TraceLog                    = 184, // _SC_TRACE_LOG

    Level1ICacheSize            = 185, // _SC_LEVEL1_ICACHE_SIZE
    Level1ICacheAssoc           = 186, // _SC_LEVEL1_ICACHE_ASSOC
    Level1ICacheLinesize        = 187, // _SC_LEVEL1_ICACHE_LINESIZE
    Level1DCacheSize            = 188, // _SC_LEVEL1_DCACHE_SIZE
    Level1DCacheAssoc           = 189, // _SC_LEVEL1_DCACHE_ASSOC
    Level1DCacheLinesize        = 190, // _SC_LEVEL1_DCACHE_LINESIZE
    Level2CacheSize             = 191, // _SC_LEVEL2_CACHE_SIZE
    Level2CacheAssoc            = 192, // _SC_LEVEL2_CACHE_ASSOC
    Level2CacheLinesize         = 193, // _SC_LEVEL2_CACHE_LINESIZE
    Level3CacheSize             = 194, // _SC_LEVEL3_CACHE_SIZE
    Level3CacheAssoc            = 195, // _SC_LEVEL3_CACHE_ASSOC
    Level3CacheLinesize         = 196, // _SC_LEVEL3_CACHE_LINESIZE
    Level4CacheSize             = 197, // _SC_LEVEL4_CACHE_SIZE
    Level4CacheAssoc            = 198, // _SC_LEVEL4_CACHE_ASSOC
    Level4CacheLinesize         = 199, // _SC_LEVEL4_CACHE_LINESIZE

    Ipv6                        = 235, // _SC_IPV6 (= _SC_LEVEL1_ICACHE_SIZE + 50)
    RawSockets                  = 236, // _SC_RAW_SOCKETS

    V7Ilp32Off32                = 237, // _SC_V7_ILP32_OFF32
    V7Ilp32Offbig               = 238, // _SC_V7_ILP32_OFFBIG
    V7Lp64Off64                 = 239, // _SC_V7_LP64_OFF64
    V7LpbigOffbig               = 240, // _SC_V7_LPBIG_OFFBIG

    SsReplMax                   = 241, // _SC_SS_REPL_MAX
    TraceEventNameMax           = 242, // _SC_TRACE_EVENT_NAME_MAX
    TraceNameMax                = 243, // _SC_TRACE_NAME_MAX
    TraceSysMax                 = 244, // _SC_TRACE_SYS_MAX
    TraceUserEventMax           = 245, // _SC_TRACE_USER_EVENT_MAX

    XOpenStreams                = 246, // _SC_XOPEN_STREAMS
    ThreadRobustPrioInherit     = 247, // _SC_THREAD_ROBUST_PRIO_INHERIT
    ThreadRobustPrioProtect     = 248, // _SC_THREAD_ROBUST_PRIO_PROTECT
    Minsigstksz                 = 249, // _SC_MINSIGSTKSZ
    Sigstksz                    = 250  // _SC_SIGSTKSZ
}