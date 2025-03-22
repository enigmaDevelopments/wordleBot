using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Net;

class Node
{
    public Dictionary<char,Node> Childeren = new Dictionary<char, Node>(26);
    public Node(){}
}

class Info
{
    public char data;
    public byte position;
    public Info(char data, byte position)
    {
        this.data = data;
        this.position = position;
    }
    public override int GetHashCode()
    {
        return ((int)data-97) <<3 | (int)position;
    }
    public override bool Equals(object obj)
    {
        return GetHashCode() == obj.GetHashCode();
    }
}


class Trie
{
    private static Node root = new Node();
    private Dictionary<byte,char> correct = new Dictionary<byte, char>(5);
    private HashSet<char> incorrect = new HashSet<char>(26);
    private Dictionary<char,byte> unknownMin = new Dictionary<char, byte>(26);
    private Dictionary<char, byte> unknownMax = new Dictionary<char, byte>(26);
    private HashSet<Info> unknown = new HashSet<Info>(255);


    public Trie(){}
    public Trie(Trie trie)
    {
        correct = new Dictionary<byte, char>(trie.correct);
        incorrect = new HashSet<char>(trie.incorrect);
        unknownMin = new Dictionary<char, byte>(trie.unknownMin);
        unknownMax = new Dictionary<char, byte>(trie.unknownMax);
        unknown = new HashSet<Info>(trie.unknown);
    }
    public Trie(string file)
    {
        make(file);
    }
    public static void make(string file)
    {
        using (StreamReader sr = new StreamReader(file))
        {
            while (!sr.EndOfStream)
            {
                string ln = sr.ReadLine();
                Node current = root;
                foreach (char c in ln)
                {

                    if (current.Childeren.ContainsKey(c))
                        current = current.Childeren[c];
                    else
                    {
                        Node next = new Node();
                        current.Childeren.Add(c, next);
                        current = next;
                    }
                }
            }
        }
    }
    public Trie Copy()
    {
        return new Trie(this);
    }

    public string[] GetPossible()
    {
        List<string> output = new List<string>();
        Dictionary<char, byte> map = new Dictionary<char, byte>(26);
        foreach (char c in "abcdefghijklmnopqrstuvwxyz")
            map.Add(c,0);
        Possible(root, output, map);
        return output.ToArray();
    }
    private void Possible(Node current,  List<string> list, Dictionary<char, byte> charMap, string str = "")
    {
        byte depth = (byte)str.Length;
        byte reverseDepth = (byte)(5 - depth);
        byte neededMoves = 0;

        foreach (var c in unknownMin) 
        {
            byte toAdd = (byte)(c.Value - charMap[c.Key]);
            if (toAdd <= 0)
                continue;
            neededMoves += toAdd;
            if (reverseDepth < neededMoves)
                return;
        }
        foreach (var c in unknownMax)
            if (c.Value < charMap[c.Key])
                return;
        if (current.Childeren.Count == 0)
        { 
            list.Add(str);
            return;
        }
        if (correct.ContainsKey(depth))
        {
            char c = correct[depth];
            if (current.Childeren.ContainsKey(c))
            {
                Dictionary<char, byte> map = new Dictionary<char, byte>(charMap);
                map.TryAdd(c, 0);
                map[c]++;
                Possible(current.Childeren[c], list, map, str + c);
            }
            return;
        }
        foreach (char c in current.Childeren.Keys)
        {
             if (incorrect.Contains(c) || unknown.Contains(new Info(c,depth)))
                continue;
            Dictionary<char, byte> map = new Dictionary<char, byte>(charMap);
            map.TryAdd(c, 0);
            map[c]++;
            Possible(current.Childeren[c], list,map, str + c);
        }
    }

    public bool MakeMove(string word, string data)
    {
        word = word.ToLower();
        data = data.ToLower();
        if (word.Length != 5 || data.Length != 5)
            return false;
        Dictionary<char,byte> count = new Dictionary<char, byte>(26);
        List<char> unkownPos = new List<char>();
        List<Info> incorrectPos = new List<Info>();
        for (byte i = 0; i < data.Length; i++)
        {
            if (data[i] == 'o')
            {
                if (!count.ContainsKey(word[i]))
                    count.Add(word[i], 0);
                count[word[i]]++;
                correct.TryAdd(i, word[i]);
            }
            else if (data[i] == 'x')
            {
                incorrectPos.Add(new Info(word[i],i));
            }
            else if (data[i] == '-')
            {
                count.TryAdd(word[i], 0);
                unkownPos.Add(word[i]);
                count[word[i]]++;
                unknown.Add(new Info(word[i], i));
            }
        }
        foreach (char c in unkownPos)
        {
            unknownMin.TryAdd(c, 0);
            unknownMin[c] = Math.Max(unknownMin[c], count[c]);
        }
        foreach (Info i in incorrectPos) 
        {
            if (unknownMin.ContainsKey(i.data))
            {
                byte newVal;
                unknownMax.TryAdd(i.data, 4);
                if (count.TryGetValue(i.data, out newVal))
                    newVal = 4;
                unknownMax[i.data] = Math.Min(unknownMax[i.data], newVal);
                unknown.Add(i);
                continue;
            }
            incorrect.Add(i.data);
        }
        return true;
    }

    private int Score(string guess, string possble)
    {
        char[] data = "xxxxx".ToCharArray();
        List<char> list = possble.ToList();
        for (int i = 4; i >= 0; i--)
        {
            if (possble[i] == guess[i])
            {
                data[i] = 'o';
                list.RemoveAt(i);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (list.Contains(guess[i]))
            {
                list.Remove(guess[i]);
                data[i] = '-';
            }
        }
        Trie trie = this.Copy();
        trie.MakeMove(guess, new string(data));
        return trie.GetPossible().Length;
    }

    public string MinMax(string file)
    {
        string[] possible = GetPossible();
        if (possible.Length <= 2)
            return possible[0];
        int min = int.MaxValue;
        string output = "";
        using (StreamReader sr = new StreamReader(file))
        {
            while (!sr.EndOfStream)
            {
                int max = 0;
                string ln = sr.ReadLine();
                foreach (string s in possible)
                {
                    max = Math.Max(Score(ln,s), max);
                }
                if (max < min)
                {
                    min = max;
                    output = ln;
                }
                Console.WriteLine(ln);
            }
        }
        return output;
    }

    public bool Contains(string word)
    {
        Node current = root;
        foreach (char c in word)
        {
            if (!current.Childeren.ContainsKey(c))
                return false;
            current = current.Childeren[c];
        }
        return true;
    }
}
