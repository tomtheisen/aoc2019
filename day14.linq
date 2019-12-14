<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

struct Supply {
	public string Material;
	public long Quantity;

	public Supply(string s) {
		var parts = s.Trim().Split(' ');
		Quantity = long.Parse(parts[0]);
		Material = parts[1];
	}
	
	public Supply(long quantity, string material) 
		=> (Material, Quantity) = (material, quantity);
	
	public static Supply operator * (Supply s, long n) 
		=> new Supply(n * s.Quantity, s.Material);
}

struct Production {
	public List<Supply> Inputs;
	public Supply Output;
	
	public Production(string s) {
		var parts = s.Split(" => ");
		Output = new Supply(parts[1]);
		Inputs = parts[0].Split(',').Select(s => new Supply(s)).ToList();
	}
}

void Main() {
	var prod = GetAocLines()
		.Select(line => new Production(line))
		.ToDictionary(s => s.Output.Material);

	long GetUsedOre(long fuel) {
		var have = new DefaultDictionary<string, long>(_ => 0);
		long usedOre = 0;
		var needed = new Stack<Supply>();
		needed.Push(new Supply(fuel, "FUEL"));

		while (needed.Any()) {
			var toSupply = needed.Pop();
			var recipe = prod[toSupply.Material];
			long toMake = toSupply.Quantity - have[toSupply.Material];
			long batches = Math.Max(0, (long)Ceiling(1.0 * toMake / recipe.Output.Quantity));
			have[toSupply.Material] += batches * recipe.Output.Quantity - toSupply.Quantity;

			foreach (var input in recipe.Inputs) {
				if (input.Material == "ORE") usedOre += input.Quantity * batches;
				else needed.Push(input * batches);
			}
		}
		return usedOre;
	}

	Console.WriteLine(GetUsedOre(1));
	
	const long maxOre = 1_000_000_000_000;
	long hi;
	for (hi = 1; GetUsedOre(hi) < maxOre; hi *= 2) ;
	long lo = hi / 2;
	
	while (lo + 1 < hi) {
		long mid = lo + hi >> 1;
		if (GetUsedOre(mid) > maxOre) hi = mid;
		else lo	= mid;
	}
	Console.WriteLine(lo);
}
