<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

const long DeckSize = 119_315_717_514_047; // prime via https://primes.utm.edu/curios/includes/primetest.php
const long Shuffles = 101_741_582_076_661;

// All shuffling operations and combinations thereof can be represented by a scale and shift.
// For a given starting position pos
//   
//     newpos = (pos * scale + shift) % decksize

// Net effect to position of arbitrary card for single shuffle
BigInteger netmul = 1, netadd = 0;

foreach (var tech in GetAocLines()) {
	if (tech == "deal into new stack") {
		// revserse
		// -1 - (an + b)
		// = (-an - b - 1)s
		netmul = -netmul;
		netadd = ~netadd % DeckSize;
	}
	else if (tech.StartsWith("deal with increment")) {
		// multiply
		// (an + b) * c
		// = (acn + bc)
		int inc = int.Parse(tech.Split().Last());
		netmul = netmul * inc % DeckSize;
		netadd = netadd * inc % DeckSize;
	}
	else if (tech.StartsWith("cut")) {
		// subtract
		// (an + b) - c
		// = (an + b - c)
		int cut = int.Parse(tech.Split().Last());
		netadd = (netadd - cut) % DeckSize;
	}
	else throw new ArgumentOutOfRangeException();
}

if (netadd < 0) netadd += DeckSize;
if (netmul < 0) netmul += DeckSize;
new { netmul, netadd }.Dump("single shuffle basis");

// powers of 2 shuffles
// each shuffle in the list is the result of repeating the last one twice
var powerShuffles = new List<(BigInteger mul, BigInteger add)> { (netmul, netadd) };
for (var i = 0; i < 63; i++) {
	var (mul, add) = powerShuffles.Last();
	powerShuffles.Add((mul * mul % DeckSize, (mul * add + add) % DeckSize));
}

netmul = 1;
netadd = 0;

// to build up a single shufle operation, for each set bit in the target shuffle quantity,
// apply the corresponding "power shuffle" to the cumulative shuffle operation 
for (int i = 0; i < 63; i++) {
	if ((Shuffles >> i & 1) == 0) continue; // unset bit; skip it
	
	// net result of applying total shuffle and then powershuffle
	(netmul, netadd) = (
		netmul * powerShuffles[i].mul % DeckSize, 
		(netmul * powerShuffles[i].add + netadd) % DeckSize
	);
}

// modular multiplicative inverse
// n * inv(n) % size == 1
// this way, multiplying by inv(n) is an exact integer division in "mod space"
long Inverse(BigInteger n) => (long)BigInteger.ModPow(n, DeckSize - 2, DeckSize);

// what number is on the card that ends up in targt position?

// target = solution * netmul + netadd (mod decksize)
// (target - netadd) / netmul = solution (mod decksize)
// (target - netadd) * inv(netmul) = solution (mod decksize)

BigInteger targetPosition = 2020;
var cardValue = (targetPosition - netadd + DeckSize) * Inverse(netmul) % DeckSize;
WriteLine($"{Shuffles} shuffles");
WriteLine($"deck[{targetPosition}] = {cardValue}");
