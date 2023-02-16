namespace ChatApp;

class Program
{
    private static void Main()
    {
        Console.Clear();
        Console.Write("Iniciar o servidor [s] ou o cliente [c]? ");
        string option = Console.ReadLine();

        if (option == "s")
        {
            Server.Start();
            Console.Title = "Servidor";
        }
        else
        {
            Client.Connect();
            Console.Title = "Cliente";
        }

        Console.ReadLine();
    }
}