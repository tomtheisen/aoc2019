<Query Kind="Statements" />

#load ".\helpers.linq"
#load ".\intcode.linq"

List<List<int>> GetPermutations(List<int> arr) {
	var result = new List<List<int>>();
	if (arr.Count == 0) result.Add(new List<int>());
	for (int i = 0; i < arr.Count; i++) {
		var suffixes = GetPermutations(arr.Take(i).Concat(arr.Skip(i+1)).ToList());
		result.AddRange(suffixes.Select(perm => perm.Prepend(arr[i]).ToList()));
	}
	return result;
}

int BestOutput(List<int> phases) {
	var permuations = GetPermutations(phases);
	var machines = "ABCDE"
		.Select(c => new IntCodeMachine(GetAocIntegers(), c.ToString()))
		.ToArray();

	int result = int.MinValue;
	machines[0].UnhandledInput += num => result = Math.Max(result, num);

	for (int i = 0; i < machines.Length; i++)
		machines[i].Outputting += machines[-~i % machines.Length].TakeInput;
		
	var cluster = new IntCodeCluster(machines);

	foreach (var permutation in permuations) {
		cluster.Reset();
		for (int i = 0; i < machines.Length; i++) machines[i].TakeInput(permutation[i]);
		machines[0].TakeInput(0);
		cluster.Run();
	}
	return result;
}

BestOutput(Enumerable.Range(0, 5).ToList()).Dump();
BestOutput(Enumerable.Range(5, 5).ToList()).Dump();
