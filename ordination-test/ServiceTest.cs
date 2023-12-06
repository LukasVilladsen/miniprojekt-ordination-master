 namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }
    //opret daglig fast ordination
    //tester oprettelse af daglig ordination, og hvor mange der så er 
   
    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            -2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));
        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            0, 3, 7, 1, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(3, service.GetDagligFaste().Count());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        throw new ArgumentNullException();
        Console.WriteLine("Her kommer der ikke en exception. Testen fejler.");
        
    }

   //test case total dosis
   //skal tjekke om total dosis er rigtig
    public class DagligFastTest
    {

        
        Laegemiddel laegemiddel = new Laegemiddel("TestMiddel", 1, 1, 1, "styk");

        [TestMethod]
        public void TestDoegnDosisNormal()
        {
            DagligFast ordination = new DagligFast(DateTime.Now, DateTime.Now.AddDays(7), laegemiddel, -3, 0, 0, 0);

            double forventetDosis = -3;

            Assert.AreEqual(forventetDosis, ordination.doegnDosis());
        }

   
    }

}

