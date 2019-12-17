<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

string Solve(string input, int messageStart) {
	var sig = input.Select(c => (int)(c - '0')).ToArray();
	int[] newsig = new int[sig.Length], sum = new int[sig.Length + 1];
	
	for (int phase = 0; phase < 100; phase++) {
		for (int i = messageStart + 1; i < sum.Length; i++) {
			sum[i] = sum[i - 1] + sig[i - 1];
		}
		
		for (int run = messageStart + 1; run <= sig.Length; run++) {
			int e = 0;
			for (int j = run - 1; j < sig.Length; j += run * 4) {
				e += sum[Min(sig.Length, j + run)] - sum[j];
				e -= sum[Min(sig.Length, j + run * 3)] - sum[Min(sig.Length, j + run * 2)];
			}
			newsig[run - 1] = Abs(e % 10);
		}
		(sig, newsig) = (newsig, sig);
	}
	
	var message = sig.Skip(messageStart).Take(8);
	return string.Concat(message);
}

string input = GetAocLines()[0];
Solve(input, 0).Dump();
Solve(string.Concat(Enumerable.Repeat(input, 10_000)), int.Parse(input.Substring(0, 7))).Dump();
