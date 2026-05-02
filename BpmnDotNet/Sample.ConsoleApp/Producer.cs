namespace Sample.ConsoleApp;

internal class Producer
{
    private readonly SampleService _sampleService;

    public Producer(SampleService sampleService)
    {
        _sampleService = sampleService ?? throw new ArgumentNullException(nameof(sampleService));
    }

    public void Produce()
    {
        var keepRunning = true;
        var dataTimeProcess = DateTime.Now;

        while (keepRunning)
        {
            Console.WriteLine(@"a) start process s) send received message q) Quit");
            var key = char.ToLower(Console.ReadKey(true).KeyChar);

            switch (key)
            {
                case 'a':
                    _sampleService.StartNewProcess(dataTimeProcess);
                    break;
                case 's':
                    _sampleService.SendMessage(dataTimeProcess);
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