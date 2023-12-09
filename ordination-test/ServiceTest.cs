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
    Laegemiddel laegemiddel = new Laegemiddel("TestMiddel", 1, 1, 1, "styk");

    [TestMethod]
    public void TestDoegnDosisTotal()
    {
        DagligFast ordination = new DagligFast(DateTime.Now, DateTime.Now.AddDays(7), laegemiddel, -3, 0, 0, 0);

        double forventetDosis = -3;

        Assert.AreEqual(forventetDosis, ordination.doegnDosis());
    }


    //skal teste oprettelse af en PN
    [TestMethod]
   public void TestOpretPN()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
        
        Assert.AreEqual(4, service.GetPNs().Count());
        
        service.OpretPN(patient.PatientId, lm.LaegemiddelId,
           -23, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(5, service.GetPNs().Count());

    }
    //skal teste oprettelse af daglig skæv
    [TestMethod]
    public void TestOpretDagligSkaev()
    {
       
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
        Assert.AreEqual(1, service.GetDagligSkæve().Count());

        var skæv1 = service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, new Dosis[]
        {
        new Dosis(DateTime.Now, -2),
        new Dosis(DateTime.Now.AddHours(6), 0),
        new Dosis(DateTime.Now.AddHours(12), -5),
    
        }, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligSkæve().Count());


    }

   

    }




