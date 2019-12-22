<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

int[] GetDeck(int size) => Enumerable.Range(0, size).ToArray();

var deck = GetDeck(10_007);
const int target = 2019;

foreach (var tech in GetAocLines()) {
	if (tech == "deal into new stack") {
		Array.Reverse(deck);
	}
	else if (tech.StartsWith("deal with increment")) {
		int inc = int.Parse(tech.Split().Last());
		
		var newDeck = new int[deck.Length];
		for (int a = 0, b = 0; a < deck.Length; a++, b = (b + inc) % deck.Length) {
			newDeck[b] = deck[a];
		}
		deck = newDeck;
	}
	else if (tech.StartsWith("cut")) {
		int cut = int.Parse(tech.Split().Last());
		if (cut < 0) cut += deck.Length;
		deck = deck[cut..].Concat(deck[..cut]).ToArray();
	}
	else throw new ArgumentOutOfRangeException();
}

//Console.WriteLine(string.Join("\n", deck));
int idx = Array.IndexOf(deck, target);
Console.WriteLine($"deck[{idx}] = {deck[idx]}");
