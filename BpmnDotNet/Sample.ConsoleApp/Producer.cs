using System;
using System.Linq;
using System.Threading.Tasks;


namespace Sample.ConsoleApp;

public class Producer
{
    private readonly SampleService _sampleService;

    public Producer(SampleService sampleService)
    {
        _sampleService = sampleService ?? throw new ArgumentNullException(nameof(sampleService));
    }

    public void Produce()
    {
        var keepRunning = true;

        // _sampleService.StartNewProcess();

        while (keepRunning)
        {
            Console.WriteLine(@"a) Send 1 start \n q) Quit");
            var key = char.ToLower(Console.ReadKey(true).KeyChar);

            switch (key)
            {
                case 'a':
                    _sampleService.StartNewProcess();
                    break;
                case 's':
                    _sampleService.SendMessage();
                    break;
                case 'q':
                    Console.WriteLine("Quitting");
                    _sampleService.Stop();
                    keepRunning = false;
                    break;
            }
        }

        Console.WriteLine("Consumer listening - press ENTER to quit");
        Console.ReadLine();
    }


}