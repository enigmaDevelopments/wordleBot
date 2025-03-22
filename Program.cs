Trie wordle = new Trie("wordle-answers-alphabetical.txt");

while (true)
{
    string word = wordle.MinMax("valid-wordle-words.txt");
    Console.WriteLine(word);
    string data;
    do
    {
        Console.Write("Enter results: ");
        data = Console.ReadLine();
    } while (!wordle.MakeMove(word, data));
}
