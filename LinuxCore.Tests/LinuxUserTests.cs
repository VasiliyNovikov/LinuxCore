using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxUserTests
{
    [TestMethod]
    public void LinuxUser_Get_ByName_Root_Returns_Uid_Zero()
    {
        var user = LinuxUser.Get("root");
        Assert.IsNotNull(user);
        Assert.AreEqual(0u, user.Id);
        Assert.AreEqual("root", user.Name);
    }

    [TestMethod]
    public void LinuxUser_Get_ById_Zero_Returns_Root()
    {
        var user = LinuxUser.Get(0u);
        Assert.IsNotNull(user);
        Assert.AreEqual("root", user.Name);
        Assert.AreEqual(0u, user.Id);
    }

    [TestMethod]
    public void LinuxUser_Get_ByName_NonExistent_Returns_Null()
    {
        var user = LinuxUser.Get("__nonexistent_user_12345__");
        Assert.IsNull(user);
    }

    [TestMethod]
    public void LinuxUser_Get_ById_NonExistent_Returns_Null()
    {
        var user = LinuxUser.Get(uint.MaxValue);
        Assert.IsNull(user);
    }

    [TestMethod]
    public void LinuxUser_Current_Returns_NonNull_With_Valid_Properties()
    {
        var user = LinuxUser.Current;
        Assert.IsNotNull(user);
        Assert.IsNotNull(user.Name);
        Assert.AreNotEqual(string.Empty, user.Name);
    }

    [TestMethod]
    public void LinuxUser_CurrentId_Matches_Current_Id()
    {
        Assert.AreEqual(LinuxUser.CurrentId, LinuxUser.Current!.Id);
    }

    [TestMethod]
    public void LinuxUser_IsRoot_Consistent_With_CurrentId()
    {
        Assert.AreEqual(LinuxUser.CurrentId == LinuxUser.RootUserId, LinuxUser.IsRoot);
    }

    [TestMethod]
    public void LinuxUser_Current_Group_Matches_GroupId()
    {
        var user = LinuxUser.Current!;
        var group = user.Group;
        Assert.IsNotNull(group);
        Assert.AreEqual(user.GroupId, group.Id);
    }

    [TestMethod]
    public void LinuxUser_Get_RoundTrip_ById_And_ByName()
    {
        var byId = LinuxUser.Get(0u)!;
        var byName = LinuxUser.Get(byId.Name)!;
        Assert.AreEqual(byId.Id, byName.Id);
        Assert.AreEqual(byId.Name, byName.Name);
        Assert.AreEqual(byId.GroupId, byName.GroupId);
        Assert.AreEqual(byId.Home, byName.Home);
        Assert.AreEqual(byId.Shell, byName.Shell);
    }

    [TestMethod]
    public void LinuxUser_Current_Matches_Id_Command()
    {
        var expectedUid = uint.Parse(Script.Run("id", "-u"), CultureInfo.InvariantCulture);
        var expectedName = Script.Run("id", "-un");

        var user = LinuxUser.Current!;
        Assert.AreEqual(expectedUid, user.Id);
        Assert.AreEqual(expectedName, user.Name);
    }

    [TestMethod]
    public void LinuxUser_Root_Home_Matches_Getent()
    {
        // Cross-validate against getent to avoid hard-coding root's home directory
        var getentLine = Script.Run("getent", "passwd", "root");
        var expectedHome = getentLine.Split(':')[5];

        var root = LinuxUser.Get(0u)!;
        Assert.AreEqual(expectedHome, root.Home);
    }
}