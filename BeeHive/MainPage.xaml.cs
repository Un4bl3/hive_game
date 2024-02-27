
using System.Data;
using System.Numerics;

namespace BeeHive;

abstract class Bee
{
    
    public abstract decimal CostPerShift { get; }
    public string Job { get; private set; }

    public Bee(string job)
    {
        Job = job;
    }
    public virtual bool WorkTheNextShift()
    {
        if (HoneyVault.ConsumeHoney(CostPerShift))
            return true;
        else
            return false;
    }
}

class NectarCollector : Bee
{
    public NectarCollector() : base("Nectar Collector") { }
    public override decimal CostPerShift
    {
        get { return Constants.NECTAR_COLLECTOR_COST;}
    }
    public override bool WorkTheNextShift()
    {
        HoneyVault.CollectNectar(Constants.NECTAR_COLLECTED_PER_SHIFT);
        return base.WorkTheNextShift();
    }
}
static class HoneyVault
{
    private static decimal Honey = Constants.INITIAL_HONEY;
    private static decimal Nectar = Constants.INITIAL_NECTAR;
    public static string ShiftReport { get; }
    internal static void Reset()
    {
        Honey = Constants.INITIAL_HONEY;
        Nectar = Constants.INITIAL_NECTAR;
    }
 
    public static void CollectNectar(decimal NectarCollected)
    {
        if (NectarCollected > 0m)
        {
            Nectar += NectarCollected;
        }
    }
    public static void ConvertNectarToHoney(decimal amount)
    {
        decimal nectarToConvert = amount;
        if (nectarToConvert > Nectar) nectarToConvert = Nectar;
        Nectar -= nectarToConvert;
        Honey += nectarToConvert * Constants.NECTAR_CONVERSION_RATIO;
    }
    public static bool ConsumeHoney(decimal amount)
    {
        if (Honey >= amount)
        {
            Honey -= amount;
            return true;
        }
        else
            return false;
    }
    public static string StatusReport
    {

        get
        {
            string status = $"{Honey:0.00} units of honey\n" + $"{Nectar:0.00} units of nectar";
            string warnings = "";
            if (Honey < Constants.LOW_LEVEL_WARNING) 
                warnings += "\nLOW HONEY - ADD A HONEY MANUFACTURER";
            if (Nectar < Constants.LOW_LEVEL_WARNING)
                warnings += "\nLOW NECTAR - ADD A NECTAR COLLECTOR";
            return status + warnings;
        }
    }

}
class Queen : Bee
{
    private Bee[] workers = Array.Empty<Bee>();
    private decimal eggs = 0;
    private decimal unassignedWorkers = 3;
    
    public bool CanAssignWorkers { get { return unassignedWorkers >= 1; } }
    public string StatusReport { get; private set; }
    public Queen() : base("Queen")
    {
        AssignBee("Egg Care");
        AssignBee("Nectar Collector");
        AssignBee("Honey Manufacturer");

    }
    public override decimal CostPerShift
    {
        get { return Constants.QUEEN_COST_PER_SHIFT; }
    }
    public void AssignBee(string Picked)
    {
        switch (Picked)
        {
            case "Egg Care":
                AddWorker(new EggCare(this));
                break;
            case "Nectar Collector":
                AddWorker(new NectarCollector());
                break;
            case "Honey Manufacturer":
                AddWorker(new HoneyManufacturer());
                break;
            default: 
            break;
        }
        UpdateStatusReport(true);
    }
    private void AddWorker(Bee worker)
    {
        if (unassignedWorkers >= 1)
        {
            unassignedWorkers--;
            Array.Resize(ref workers, workers.Length + 1);
            workers[workers.Length - 1] = worker;
        }
    }
    public override bool WorkTheNextShift ()
    {
        eggs += Constants.EGGS_PER_SHIFT;
        bool allWorkersDidTheirJobs = true;
        foreach (Bee worker in workers)
        {
            if (!worker.WorkTheNextShift())
            {
                allWorkersDidTheirJobs = false;
            }
        }
        HoneyVault.ConsumeHoney(unassignedWorkers * Constants.HONEY_PER_UNASSIGNED_WORKER);
        UpdateStatusReport(allWorkersDidTheirJobs);
        return base.WorkTheNextShift();
    }
    private string WorkerStatus(string job)
    {
        int count = 0;
        foreach(Bee worker in workers)
            if (worker.Job == job) count++;
        string s = "s";
        if (count == 1) s = "";
        return $"{count} {job} bee{s}";
    }
    private void UpdateStatusReport(bool allWorkersDidTheirJobs)
    {
        StatusReport = $"Vault report:\n{HoneyVault.StatusReport}\n" +
        $"\nEgg count: {eggs:0.00}\nUnassigned workers: {unassignedWorkers:0.00}\n" +
        $"{WorkerStatus("Nectar Collector")}\n{WorkerStatus("Honey Manufacturer")}" +
        $"\n{WorkerStatus("Egg Care")}\nTOTAL WORKERS: {workers.Length}";
        if (!allWorkersDidTheirJobs)
            StatusReport += "\nWARNING: NOT ALL WORKERS DID THEIR JOBS";
    }
    public void ReportEggConversion(decimal eggsToConvert)
    {
        if (eggs >= eggsToConvert)
        {
            eggs -= eggsToConvert;
            unassignedWorkers += eggsToConvert;
        }
    }
}
  
class EggCare : Bee
{
    private Queen queen;
    public EggCare(Queen queen) : base("Egg Care")
    {
        this.queen = queen;
    }
    public override decimal CostPerShift
    {
        get { return Constants.EGG_CARE_COST; }
    }
    public override bool WorkTheNextShift()
    {
        queen.ReportEggConversion(Constants.CARE_PROGRESS_PER_SHIFT);
        return base.WorkTheNextShift();
    }
}

class HoneyManufacturer : Bee
{
    
    public HoneyManufacturer() : base("Honey Manufacturer") { }
    public override decimal CostPerShift {
        get { return Constants.HONEY_MANUFACTURER_COST;}
    }
    public override bool WorkTheNextShift()
    {
        HoneyVault.ConvertNectarToHoney(Constants.NECTAR_PROCESSED_PER_SHIFT);
        return base.WorkTheNextShift();
    }
}


static class Constants
{
    public const decimal QUEEN_COST_PER_SHIFT = 2.15m;
    public const decimal EGGS_PER_SHIFT = 0.45m;
    public const decimal LOW_LEVEL_WARNING = 10m;
    public const decimal HONEY_PER_UNASSIGNED_WORKER = 0.5m;
    public const decimal NECTAR_COLLECTOR_COST = 1.95m;
    public const decimal NECTAR_COLLECTED_PER_SHIFT = 33.25m;
    public const decimal HONEY_MANUFACTURER_COST = 1.7m;
    public const decimal NECTAR_PROCESSED_PER_SHIFT = 33.15m;
    public const decimal NECTAR_CONVERSION_RATIO = .19m;
    public const decimal EGG_CARE_COST = 1.35m;
    public const decimal CARE_PROGRESS_PER_SHIFT = 0.15m;
    public const decimal INITIAL_HONEY = 25m;
    public const decimal INITIAL_NECTAR = 100m;
}





public partial class MainPage : ContentPage
{
    private Queen queen = new Queen();
	public MainPage()
	{
        InitializeComponent();

        string[] pickerValues = { "Nectar Collector", "Honey Manufacturer", "Egg Care" };
        JobPicker.ItemsSource = pickerValues;
        JobPicker.SelectedIndex = 0;

        UpdateStatusAndEnableAssignButton();
    }

    private void UpdateStatusAndEnableAssignButton()
    {
        StatusReport.Text = queen.StatusReport;
        AssignJobButton.IsEnabled = queen.CanAssignWorkers;
    }

    

    private void OutOfHoneyButton_Clicked(object sender, EventArgs e)
    {
        HoneyVault.Reset();
        queen = new Queen();
        WorkShiftButton.IsVisible = true;
        OutOfHoneyButton.IsVisible = false;
        UpdateStatusAndEnableAssignButton();
    }

    private void AssignJobButton_Clicked(object sender, EventArgs e)
    {
        queen.AssignBee(JobPicker.SelectedItem.ToString());
        UpdateStatusAndEnableAssignButton();
    }

    private void WorkShiftButton_Clicked(object sender, EventArgs e)
    {
        if(!queen.WorkTheNextShift())
        {
            WorkShiftButton.IsVisible = false;
            OutOfHoneyButton.IsVisible = true;
        }
        UpdateStatusAndEnableAssignButton();
    }
}

