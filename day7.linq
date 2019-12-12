<Query Kind="Statements">
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

BigInteger BestOutput(List<int> phases) {
	var permuations = phases.Permutations().ToList();

	BigInteger result = int.MinValue;

	// build vm cluster
    var program = GetAocBigIntegers();
    var machines = "ABCDE".Select(c => new IntCodeMachine(program, "Amp " + c));
	var cluster = new IntCodeCluster(machines);
    
	// wire the output of each machine to the input of the next
	for (int i = 0; i < cluster.Count; i++) {
        var container = new DumpContainer().Dump(cluster[i].Name + " Last Output");
        cluster[i].Outputting += num => container.Content = num;
		cluster[i].Outputting += cluster[-~i % cluster.Count].TakeInput;
    }
	
	// input received after termination of the first machine goes to output
	cluster[0].UnhandledInput += num => result = BigInteger.Max(result, num);

	// run the cluster for each configuration of phases
	foreach (var perm in permuations) {
		cluster.Reset();
		for (int i = 0; i < cluster.Count; i++) cluster[i].TakeInput(perm[i]);
		cluster[0].TakeInput(0);
        cluster.RunConcurrently();
	}
	return result;
}

BestOutput(Enumerable.Range(0, 5).ToList()).Dump();
BestOutput(Enumerable.Range(5, 5).ToList()).Dump();