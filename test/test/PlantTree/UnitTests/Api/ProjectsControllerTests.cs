using NUnit.Framework;
using PlantTree.Controllers.Api;
using PlantTree.Data;
using PlantTree.Infrastructure.Common;

namespace test.PlantTree.UnitTests.Api
{
    [TestFixture]
    public class ProjectsControllerTests
    {
        private ProjectsController _projectsControllser;
        private SqliteTestDb _testDb;
        private AppDbContext _context;

        [SetUp]
        public void Init()
        {
            _testDb = new SqliteTestDb();
            var options = _testDb.Options;
            _context = new AppDbContext(options, new AppDbContextCache());

            _projectsControllser = new ProjectsController();
        }

        [TearDown]
        public void CleanUp()
        {
            
        }
    }
}