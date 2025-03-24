Trie wordle = new Trie("wordle-answers-alphabetical.txt");

string word = "crane";
while (true)
{
    Console.WriteLine(word);
    string data;
    do
    {
        Console.Write("Enter results: ");
        data = Console.ReadLine();
    } while (!wordle.MakeMove(word, data));
    while (true)
        try
        {
            word = wordle.MinMax("valid-wordle-words.txt");
            break;
        }
        catch
        {
            Console.WriteLine("error");
        }
}
