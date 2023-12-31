using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;
using Microsoft.Extensions.Logging;

namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db) {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget nyt data i databasen, hvis det er nødvendigt.
    /// </summary>
    public void SeedData() {

        // Patients
        Patient[] patients = new Patient[7];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 63.4);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 89.4);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);
            patients[5] = new Patient("110300-4577", "Lille Mads", 10.7);
            patients[6] = new Patient("120197-9825", "Store Jonna", 152.7);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.Patienter.Add(patients[5]);
            db.Patienter.Add(patients[6]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[6];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null) {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            ordinationer[0] = new PN(new DateTime(2021, 1, 1), new DateTime(2021, 1, 12), 123, lm[1]);    
            ordinationer[1] = new PN(new DateTime(2021, 2, 12), new DateTime(2021, 2, 14), 3, lm[0]);    
            ordinationer[2] = new PN(new DateTime(2021, 1, 20), new DateTime(2021, 1, 25), 5, lm[2]);    
            ordinationer[3] = new PN(new DateTime(2021, 1, 1), new DateTime(2021, 1, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2021, 1, 10), new DateTime(2021, 1, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2021, 1, 23), new DateTime(2021, 1, 24), lm[2]);
            
            ((DagligSkæv) ordinationer[5]).doser = new Dosis[] { 
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)        
            }.ToList();
            

            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);

            db.SaveChanges();
        }
    }

    
    public List<PN> GetPNs() {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste() {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)            
            .Include(o => o.NatDosis)            
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve() {
        return db.DagligSkæve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter() {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler() {
        return db.Laegemiddler.ToList();
    }
    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato)
    {

        // Find patienten og lægemidlet baseret på ID'erne
        Patient patient = db.Patienter.Find(patientId);
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId);

        // Tjek om patient og lægemiddel findes
        if (patient == null || laegemiddel == null)
            {

            return null;
            }

            // Opret en ny PN-ordination
            var pn = new PN(startDato, slutDato, antal, laegemiddel);

            // Manuelt tilføj patienten og lægemidlet til ordinationen
        
            pn.laegemiddel = laegemiddel;

            // Gem ordinationen i konteksten
            patient.ordinationer.Add(pn);
            db.SaveChanges();

            return pn;
        
       
    }




    public DagligFast OpretDagligFast(int patientId, int laegemiddelId,
     double antalMorgen, double antalMiddag, double antalAften, double antalNat,
     DateTime startDato, DateTime slutDato)
    {

        Patient patient = db.Patienter.Find(patientId);
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId);

        if (patient == null || laegemiddel == null)
        {
            // Patient eller lægemiddel ikke fundet
            return null;
        }

        // Opret en ny instans af DagligFast
        DagligFast dagligFast = new DagligFast(startDato, slutDato, laegemiddel, antalMorgen, antalMiddag, antalAften, antalNat);

        // Tilknyt ordinationen til patienten
        patient.ordinationer.Add(dagligFast);

        // Gem ændringer i databasen
        db.SaveChanges();

        return dagligFast;
    }


    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato)
    {

        Patient patient = db.Patienter.Find(patientId);
        Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId);

        if (patient == null || laegemiddel == null)
        {
            // Patient eller lægemiddel ikke fundet
            return null;
        }

        // Opret en ny instans af DagligSkæv
        DagligSkæv dagligSkæv = new DagligSkæv(startDato, slutDato, laegemiddel, doser);

        // Tilknyt ordinationen til patienten
        patient.ordinationer.Add(dagligSkæv);

        // Gem ændringer i databasen
        db.SaveChanges();

        return dagligSkæv;
    }


    public string AnvendOrdination(int id, Dato dato)
    {
        var pnOrdination = db.PNs
            .Include(p => p.dates)
            .Include(p => p.laegemiddel)
            .FirstOrDefault(p => p.OrdinationId == id);

        if (pnOrdination != null)
        {
            bool success = pnOrdination.givDosis(dato);

            if (success)
            {
                db.SaveChanges();
                return $"Dosis tilføjet for PN-ordination med ID {id} på dato {dato.dato.ToShortDateString()}";
            }
            else
            {
                return $"Fejl: Datoen {dato.dato.ToShortDateString()} er uden for gyldighedsperioden for PN-ordinationen med ID {id}";
            }
        }

        return $"Fejl: PN-ordination med ID {id} blev ikke fundet";
    }


    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
	/// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
	public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId)
    {
        // Find patienten og lægemidlet baseret på ID'erne
        var patient = db.Patienter.Find(patientId);
        var laegemiddel = db.Laegemiddler.Find(laegemiddelId);

        // Tjek om patient og lægemiddel findes
        if (patient == null || laegemiddel == null)
        {
            // Håndter fejl, f.eks. kast en exception eller returner -1
            return -1;
        }

        // Simpel beregning af dosis baseret på patientens vægt
        double dosis;

        if (patient.vaegt < 25)
        {
            dosis = patient.vaegt * laegemiddel.enhedPrKgPrDoegnLet;
        }
        else if (patient.vaegt >= 25 && patient.vaegt <= 120)
        {
            dosis = patient.vaegt * laegemiddel.enhedPrKgPrDoegnNormal;
        }
        else // patient.vaegt > 120
        {
            dosis = patient.vaegt * laegemiddel.enhedPrKgPrDoegnTung;
        }

        return dosis;
    }




}