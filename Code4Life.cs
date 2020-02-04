using Internal;
using System.Reflection;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public enum MoleculeType {A,B,C,D,E};

public interface Module{
    public abstract void GetDecision(Robot robot); 
}

public class StartPoint : Module{

    public override void GetDecision(Robot robot)
    {
        robot.GoTo("SAMPLES");
    }
}

public class Laboratory : Module{

    public override void GetDecision(Robot robot)
    {
        foreach(Sample sample in robot.samples)
                {
                    if (CanResearchSample(sample))
                    {
                        robot.Connect(sample.id);
                        return;
                    }                    
                }
                
                if (!SamplesDoable())
                {
                    if (robot.samples.Count < 3)
                    {
                        GoTo("SAMPLES");
                        return;
                    }
                    else
                    {
                        GoTo("DIAGNOSIS");
                        return;
                    }
                }
                else 
                {
                    GoTo("MOLECULES");
                    return;
                }
            return;
    }  
} 

public class Diagnosis : Module{

    public override void GetDecision(Robot robot)
    {
        Sample selectedSample = robot.ChooseDiagnosticableSample();
                if (selectedSample != null)
                {                    
                    Connect(selectedSample.id); 
                    return;
                }
                else
                {
                    /*
                    if (Player.robots[1].score < score + 10)
                    {
                        foreach (Sample sample in samples)
                        {
                            if(!IsGoodForProject(sample))
                            {
                                Connect(sample.id);
                                return;
                            }                        
                        }
                    
                        if (samples.Count < 3)
                        {
                            foreach(Sample sample in _samples)
                            {
                                if (IsGoodForProject(sample))
                                {
                                    Connect(sample.id);
                                    return;                                
                                }
                            }
                        }
                    }
                    else
                    {
                    */
                        if (robot.samples.Count < 3)
                        {
                            foreach (Sample sample in Player._samples)
                            {
                                if (robot.CanResearchSample(sample))
                                {
                                    robot.Connect(sample.id);
                                    return;
                                }
                            }
                        }
                    }
                    
                    if (robot.storage.Sum() == 10 && !robot.SamplesResearchable(false))
                    {
                        if (robot.samples.Count == 3)
                        {
                            Connect(samples.First().id);
                            return;
                        }
                        else
                        {
                            robot.GoTo("SAMPLES");
                            return;
                        }
                    }
                    else
                    {
                        if (robot.SamplesResearchable(false))
                        {                        
                            robot.GoTo("LABORATORY");
                            return;
                        }
                        else if (robot.SamplesDoable()) 
                        {                            
                            robot.GoTo("MOLECULES");
                            return;
                        }
                        else if (robot.samples.Count == 3)
                        {
                            robot.Connect(samples.First().id);
                            return;
                        }
                        else
                        {                            
                            robot.GoTo("SAMPLES");
                            return;
                        }
                    }
                //}
                return;
    }  
}

public class Samples : Module{

    public override void GetDecision(Robot robot)
    {
        Random rnd = new Random();
            
                if (robot.samples.Count < 3)
                {
                    if (robot.expertise.Sum() < 3)
                    {
                        robot.Connect(1);
                        return;
                    }
                    else if (robot.expertise.Sum() < 6)
                    {                        
                        //robot.Connect((int)rnd.Next(1,3));
                        robot.Connect(2);
                        return;
                    }
                    else if (robot.expertise.Sum() < 9)
                    {              
                        robot.Connect(2);
                        //robot.Connect((int)rnd.Next(1,4));
                        return;
                    }
                    else
                    {
                        robot.Connect(2);
                        //robot.Connect((int)rnd.Next(2,4));
                        return;
                    }
                }
                else
                {
                    robot.GoTo("DIAGNOSIS");
                    return;
                }
                return;
    }  
}

public class Molecules : Module{

    public override void GetDecision(Robot robot)
    {
        if (robot.storage.Sum() < 10)
                {
                    foreach(Sample sample in robot.samples.OrderByDescending(sample => sample.rank))
                    {                        
                        if (robot.CanResearchSample(sample))
                        {
                            robot.GoTo("LABORATORY");
                            return;
                        }                            
                        else if (robot.CanDoSample(sample))
                        {                        
                            for (int i = 0 ; i<5 ; i++)
                            {                            
                                if ((storage[i] + expertise[i]) < sample.cost[i] && Player.available[i] > 0)
                                {
                                    robot.Connect((MoleculeType)i);
                                    return;            
                                }                            
                            }                            
                        }
                    }
                    
                    robot.GoTo("DIAGNOSIS");
                    return;
                }
                else
                {                
                    if (!robot.SamplesResearchable(false))
                    {
                        robot.GoTo("DIAGNOSIS");
                        return;
                    }
                    else
                    {                    
                        robot.GoTo("LABORATORY");
                        return;
                    }
                }
                return;
    }  
}

public class Project
{
    public int[] expertise;
    
    public Project(int[] _expertise)
    {
        expertise = _expertise;
    }
}
        

public class Sample
{
    public int id;
    public int[] cost;
    public int health;
    public int rank;
    public MoleculeType gain;
    public bool diagnosticated;
    
    public Sample(int _id, int[] _cost, int _health, int _rank, string _gain)
    {
        id = _id;
        cost = _cost;
        health = _health;
        rank = _rank;
        gain = (MoleculeType)Enum.Parse(typeof(MoleculeType), _gain);    
        diagnosticated = Array.Exists(_cost, number => number == -1) ? false : true;
    }
}

public class Molecule
{    
    public MoleculeType type;
    
    public Molecule(string _type)
    {
        type = (MoleculeType)Enum.Parse(typeof(MoleculeType), _type);
    }
}

public class Robot
{
    public List<Sample> samples;
    public Module target;
    public int eta;
    public int score;
    public int[] storage;
    public int[] expertise;
    
    public Robot(Module _target, int _eta, int _score, int[] _storage, int[] _expertise)
    {
        target = _target;
        eta = _eta;
        score = _score;
        storage = _storage;
        expertise = _expertise;
        samples = new List<Sample>();
    }

    public SetModule(Module _module)
    {
        target = _module;
    }
    
    public void Update(List<Sample> _samples)
    {
        if (samples.Count == 0 && target != Module.SAMPLES)
        {
            GoTo(Module.SAMPLES);
            return;
        }
        
        target.GetDecision(this);
    }
    
    public Project ClosestProject(List<Project> projects)
    {
        int minScore = 20;
        Project cProject = projects.First();
        
        foreach(Project p in projects)
        {
            int tempScore = 0;
            for (int i = 0 ; i < 5 ; i++)
            {
                   tempScore += Math.Max(p.expertise[i] - expertise[i], 0);
            }
            
            if (tempScore < minScore)
            {
                minScore = tempScore;
                cProject = p;
            }
        }
        Console.Error.WriteLine("Projet visé : " + projects.IndexOf(cProject));
        return cProject;        
    }
    
    public bool IsGoodForProject(Sample sample)
    {
        int moleculetype = (int)sample.gain;
        if (ClosestProject(Player.projects).expertise[moleculetype] > expertise[moleculetype])
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }
    
    public bool SamplesDoable()
    {        
        foreach(Sample sample in samples)
            {                        
                if (CanDoSample(sample))
                {
                    return true;
                }
            }
            
        return false;
    }
    
    public bool SamplesResearchable(bool project)
    {
        foreach(Sample sample in samples)
            {                        
                if (CanResearchSample(sample))
                {
                    if (project)
                    { 
                        if (IsGoodForProject(sample))
                        {
                            return true;
                        }
                    }
                    else
                    {
                    return true;
                    }
                }
            }
            
        return false;
    }
        
            
    public Sample ChooseDiagnosticableSample()
    {
        List<Sample> tempSamples = samples.Where(sample => sample.diagnosticated == false).ToList();
        return tempSamples.Count == 0 ? null : tempSamples.OrderByDescending(sample => sample.health).First();
    }

    public bool CanResearchSample(Sample sample)
    {
        bool canDo = true;
        
        for (int i = 0; i<5 ; i++)
        {
            if ((storage[i] + expertise[i]) < sample.cost[i])
            {
                canDo = false;
            }
        }        
        return canDo;        
    }
    
    public bool CanDoSample(Sample sample)
    {
        bool canDo = true;
        
        for (int i = 0; i<5 ; i++)
        {               
            if ((Player.available[i] + storage[i] + expertise[i]) < sample.cost[i])
            {
                canDo = false;
            }
            Console.Error.WriteLine("En stock : " + Player.available[i] + " - Inventaire : " + storage[i] + " - Expertise :  " + expertise[i] + " - Coût : " + sample.cost[i] + " canDo : " + (bool)canDo);
             
        }        
        return canDo;
    }
    
    public void GoTo(string module)
    {
        Console.WriteLine("GOTO " + module);
    }
    
    public void Connect(int id)
    {
        Console.WriteLine("CONNECT " + id);        
    }
    
    public void Connect(MoleculeType type)
    {
        Console.WriteLine("CONNECT " + type.ToString());        
    }

    public void Wait()
    {
        Console.WriteLine("WAIT");
    }
}


/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
class Player
{
    public static int samplesTaken;
    public static int[] available;
    public static List<Project> projects;
    public static List<Robot> robots;
    public static List<Sample> samples;
    
    static void Main(string[] args)
    {
        robots = new List<Robot>();
        samples = new List<Sample>();
        projects = new List<Project>();
        
        string[] inputs;
        bool firstTurn = true;
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int a = int.Parse(inputs[0]);
            int b = int.Parse(inputs[1]);
            int c = int.Parse(inputs[2]);
            int d = int.Parse(inputs[3]);
            int e = int.Parse(inputs[4]);
            projects.Add(new Project(new int[]{a,b,c,d,e}));
        }
        

        // game loop
        while (true)
        {        
            robots.Clear();
            samples.Clear();
            
            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string target = inputs[0];
                int eta = int.Parse(inputs[1]);
                int score = int.Parse(inputs[2]);
                int storageA = int.Parse(inputs[3]);
                int storageB = int.Parse(inputs[4]);
                int storageC = int.Parse(inputs[5]);
                int storageD = int.Parse(inputs[6]);
                int storageE = int.Parse(inputs[7]);
                int expertiseA = int.Parse(inputs[8]);
                int expertiseB = int.Parse(inputs[9]);
                int expertiseC = int.Parse(inputs[10]);
                int expertiseD = int.Parse(inputs[11]);
                int expertiseE = int.Parse(inputs[12]);
                Module modTarget;
                switch (target)
                {
                    case "SAMPLES":
                    modTarget = new Samples();
                    case "LABORATORY":
                    modTarget = new Laboratory();
                    case "DIAGNOSIS":
                    modTarget = new Diagnosis();
                    case "MOLECULES":
                    modTarget = new Molecules();
                    case "START_POINT":
                    modTarget = new StartPoint();                    
                    default:
                    modTarget
                }
                
                robots.Add(new Robot(
                    modTarget, 
                    eta, 
                    score, 
                    new int[] {storageA, storageB, storageC, storageD, storageE}, 
                    new int[] {expertiseA, expertiseB, expertiseC, expertiseD, expertiseE}
                    ));
            }
            inputs = Console.ReadLine().Split(' ');
            int availableA = int.Parse(inputs[0]);
            int availableB = int.Parse(inputs[1]);
            int availableC = int.Parse(inputs[2]);
            int availableD = int.Parse(inputs[3]);
            int availableE = int.Parse(inputs[4]);
            available = new int[] {availableA, availableB, availableC, availableD, availableE};
            int sampleCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < sampleCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int sampleId = int.Parse(inputs[0]);
                int carriedBy = int.Parse(inputs[1]);
                int rank = int.Parse(inputs[2]);
                string expertiseGain = inputs[3];
                int health = int.Parse(inputs[4]);
                int costA = int.Parse(inputs[5]);
                int costB = int.Parse(inputs[6]);
                int costC = int.Parse(inputs[7]);
                int costD = int.Parse(inputs[8]);
                int costE = int.Parse(inputs[9]);
                
                Sample thisSample = new Sample(
                    sampleId, 
                    new int[] {costA, costB, costC, costD, costE},
                    health,
                    rank,
                    expertiseGain);      
                    
                switch (carriedBy)
                {
                    case -1:
                        samples.Add(thisSample);
                        break;
                    case 0:
                        robots[0].samples.Add(thisSample);
                        break;
                    case 1:
                        robots[1].samples.Add(thisSample);
                        break;
                }
                        
            }
            
            Robot myRobot = robots[0];            
                
            Console.Error.WriteLine("Storage (A B C D E) : " + String.Join(" ",myRobot.storage));
            Console.Error.WriteLine("Expert. (A B C D E) : " + String.Join(" ",myRobot.expertise));
            
            foreach(Project project in projects)
            {
                Console.Error.WriteLine("Projet " + projects.IndexOf(project) + " - Expertise : " + String.Join(" ",project.expertise));   
            }

            foreach(Sample sample in myRobot.samples.OrderByDescending(item => item.health))
                {      
                    
                Console.Error.WriteLine("Rank " + sample.rank + " - Health : " + sample.health + " - Cost (A B C D E) : " + String.Join(" ",sample.cost));
                
                }                
                
            myRobot.Update();            
        
        }
    }
}