using System;

namespace firstapp
{
    class Program
    {
        static int DemanderAge()
        {
            int age = 0;
            while (age <= 0)
            {   
                Console.Write("What is your age ?");
                string age_str = Console.ReadLine();

            
                try 
                {
                age = int.Parse(age_str);
                if (age <= 0)
                {
                    Console.WriteLine("Age must be greater than 0 !");
                }
                if (age == 0)
                {
                    Console.WriteLine("Age is null !");
                }       
                }
                catch
                {
                    Console.WriteLine("Age is not a number !");
                }
            }
            return age;
        }
        static void Main(string[] args)
        {

            // Test demander l'age
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Write("Whoami ?");
            string name = Console.ReadLine();
            name = name.Trim();
            int age = DemanderAge();
            Console.WriteLine("Hello!");
            Console.WriteLine("You are " + name + " and " + age + " Years old.");   
            int next_age = age + 1;
            Console.WriteLine("Next year you will be " + next_age + " Years old.");
            }            

                }
                catch
                {
                    Console.WriteLine("Age is not a number !");
                }
            
            }

        }
    }
}