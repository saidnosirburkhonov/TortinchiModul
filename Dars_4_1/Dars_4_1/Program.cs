namespace Dars_4_1;

public class Program
{
    static void Main(string[] args)
    {

        var age = 10;
        try
        {
            if(age < 18)
            {
                throw new AgeExsception("yosh 18dan kichik");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
