using System.Data;


Trie wordle = new Trie("wordle-answers-alphabetical.txt");



while (true)
{
    string word;
    string data;
    do
    {
        Console.Write("Enter word: ");
        word = Console.ReadLine();
        Console.Write("Enter results: ");
        data = Console.ReadLine();
    } while (!wordle.MakeMove(word, data));
    Console.WriteLine(String.Join(", ", wordle.GetPossible()));
}
