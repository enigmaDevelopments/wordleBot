using System.ComponentModel.DataAnnotations;

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
    private HashSet<Info> unknown2 = new HashSet<Info>(255);


    public Trie(){}
    public Trie(Trie trie)
    {
        correct = new Dictionary<byte, char>(trie.correct);
        incorrect = new HashSet<char>(trie.incorrect);
        unknownMin = new Dictionary<char, byte>(trie.unknownMin);
        unknownMax = new Dictionary<char, byte>(trie.unknownMax);
        unknown2 = new HashSet<Info>(trie.unknown2);
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
        Possible(root, output);
        return output.ToArray();
    }
    private void Possible(Node current,  List<string> list, string str = "")
    {
        byte depth = (byte)str.Length;
        if (current.Childeren.Count == 0)
        {
            List<char> temp = str.ToList();
            foreach (var c in unknownMin)
            {
                for (byte i = 0; i < c.Value; i++)
                {
                    if (!temp.Contains(c.Key))
                        return;
                    temp.Remove(c.Key);
                }
            }
            list.Add(str);
            return;
        }
        if (correct.ContainsKey(depth))
        {
            char c = correct[depth];
            if (current.Childeren.ContainsKey(c))
                Possible(current.Childeren[c], list, str + c);
            return;
        }
        foreach (char c in current.Childeren.Keys)
        {
             if (incorrect.Contains(c) || unknown2.Contains(new Info(c,depth)))
                continue;
            Possible(current.Childeren[c], list, str + c);
        }
    }

    public bool MakeMove(string word, string data)
    {
        word = word.ToLower();
        data = data.ToLower();
        if (word.Length != 5 || data.Length != 5)
            return false;
        Dictionary<char,byte> count = new Dictionary<char, byte>(26);
        Dictionary<char, byte> count2 = new Dictionary<char, byte>(26);
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
                if (!count2.ContainsKey(word[i]))
                    count2.Add(word[i], 0);
                count2[word[i]]++;
                incorrectPos.Add(new Info(word[i],i));
            }
            else if (data[i] == '-')
            {
                if (!count.ContainsKey(word[i]))
                    count.Add(word[i], 0);
                unkownPos.Add(word[i]);
                count[word[i]]++;
                unknown2.Add(new Info(word[i], i));
            }
        }
        foreach (char c in unkownPos)
        {
            if (!unknownMin.ContainsKey(c))
                unknownMin.Add(c, 0);
            unknownMin[c] = Math.Max(unknownMin[c], count[c]);
        }
        foreach (Info i in incorrectPos) 
        {
            if (unknownMin.ContainsKey(i.data))
            {
                if (!unknownMax.ContainsKey(i.data))
                    unknownMax.Add(i.data, 0);
                unknownMax[i.data] = Math.Min(unknownMax[i.data], count2[i.data]);
                unknown2.Add(i);
                continue;
            }
            incorrect.Add(i.data);
        }
        return true;
    }

    public string MinMax(string file)
    {
        string[] possible = GetPossible();
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
                    char[] data = "xxxxx".ToCharArray();
                    List<char> list = s.ToList();
                    for (int i = 0; i < ln.Length; i++)
                    {
                        if (list[i] == ln[i])
                            data[i] = 'o';
                    }
                    for (int i = 0; i < ln.Length; i++)
                    {
                        if (list.Contains(ln[i]))
                        {
                            list.Remove(ln[i]);
                            data[i] = '-';
                        }
                    }
                    Trie trie = this.Copy();
                    trie.MakeMove(s, new string(data));
                    max = Math.Max(trie.GetPossible().Length,max);
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
