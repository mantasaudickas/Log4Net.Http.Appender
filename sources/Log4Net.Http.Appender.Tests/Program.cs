using System;

namespace LogStashLog4Net
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var logger = log4net.LogManager.GetLogger("Logstash.Logger");

            for (int i = 0; i < 12000; i++)
            {
                log4net.LogicalThreadContext.Properties["RequestId"] = i;
                logger.InfoFormat($"Here is your log4net information entry {i}!");
            }

/*
            try
            {
                throw new Exception("Exception message is hardcoded");
            }
            catch (Exception e)
            {
                logger.Error(null, e);
            }
*/

//            logger.Error(null, new Exception("Exception message is hardcoded"));

            Console.WriteLine("Press enter to exit..");
            Console.ReadLine();
        }
    }
}
