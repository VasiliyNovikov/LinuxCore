using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinuxCore.Tests;

[TestClass]
public class LinuxGroupTests
{
    [TestMethod]
    public void LinuxGroup_Get_ByName_Root_Returns_Gid_Zero()
    {
        var group = LinuxGroup.Get("root");
        Assert.IsNotNull(group);
        Assert.AreEqual(0u, group.Id);
        Assert.AreEqual("root", group.Name);
    }

    [TestMethod]
    public void LinuxGroup_Get_ById_Zero_Returns_Root()
    {
        var group = LinuxGroup.Get(0u);
        Assert.IsNotNull(group);
        Assert.AreEqual("root", group.Name);
        Assert.AreEqual(0u, group.Id);
    }

    [TestMethod]
    public void LinuxGroup_Get_ByName_NonExistent_Returns_Null()
    {
        var group = LinuxGroup.Get("__nonexistent_group_12345__");
        Assert.IsNull(group);
    }

    [TestMethod]
    public void LinuxGroup_Get_ById_NonExistent_Returns_Null()
    {
        var group = LinuxGroup.Get(uint.MaxValue);
        Assert.IsNull(group);
    }

    [TestMethod]
    public void LinuxGroup_Get_RoundTrip_ById_And_ByName()
    {
        var byId = LinuxGroup.Get(0u)!;
        var byName = LinuxGroup.Get(byId.Name)!;
        Assert.AreEqual(byId.Id, byName.Id);
        Assert.AreEqual(byId.Name, byName.Name);
    }

    [TestMethod]
    public void LinuxGroup_Members_Is_Valid_Array()
    {
        var group = LinuxGroup.Get(0u)!;
        Assert.IsFalse(group.Members.IsDefault);
    }

    [TestMethod]
    public void LinuxGroup_Current_User_Primary_Group_Matches_Id_Command()
    {
        var expectedGroupName = Script.Run("id", "-gn");
        var expectedGid = uint.Parse(Script.Run("id", "-g"), CultureInfo.InvariantCulture);

        var user = LinuxUser.Current!;
        var group = user.Group;
        Assert.AreEqual(expectedGid, group.Id);
        Assert.AreEqual(expectedGroupName, group.Name);
    }
}