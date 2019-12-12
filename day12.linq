<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

BigInteger LCM(params BigInteger[] values) => values.Aggregate(BigInteger.One, (a, b) => a * b / BigInteger.GreatestCommonDivisor(a, b));

void Main() {
	var planets = GetAocLines().Select(line => new Planet(line)).ToArray().Dump("Initial", 0);
	Dictionary<(int, int, int, int, int, int, int, int), long>
		seenX = new Dictionary<(int, int, int, int, int, int, int, int), long>(),
		seenY = new Dictionary<(int, int, int, int, int, int, int, int), long>(),
		seenZ = new Dictionary<(int, int, int, int, int, int, int, int), long>();
		
	long loopX = 0, loopY = 0, loopZ = 0;

	checked {
		for (long step = 1; loopX * loopY * loopZ == 0; step++) {
			for (int i = 0; i < planets.Length; i++) {
				for (int j = 0; j < planets.Length; j++) {
					planets[i].VX += (int)Sign(planets[j].X - planets[i].X);
					planets[i].VY += (int)Sign(planets[j].Y - planets[i].Y);
					planets[i].VZ += (int)Sign(planets[j].Z - planets[i].Z);
				}
			}

			for (int i = 0; i < planets.Length; i++) {
				planets[i].X += planets[i].VX;
				planets[i].Y += planets[i].VY;
				planets[i].Z += planets[i].VZ;
			}
			
			var currX = (
				planets[0].X, planets[0].VX,
				planets[1].X, planets[1].VX,
				planets[2].X, planets[2].VX,
				planets[3].X, planets[3].VX);
			if (loopX == 0) {
				if (seenX.ContainsKey(currX)) {
					Console.WriteLine($"Seen x @step {step} {currX} first seen {seenX[currX]}");
					loopX = step - seenX[currX];
				}
				else seenX.Add(currX, step);
			}

			var currY = (
				planets[0].Y, planets[0].VY,
				planets[1].Y, planets[1].VY,
				planets[2].Y, planets[2].VY,
				planets[3].Y, planets[3].VY);
			if (loopY == 0) {
				if (seenY.ContainsKey(currY)) {
					Console.WriteLine($"Seen Y @step {step} {currY} first seen {seenY[currY]}");
					loopY = step - seenY[currY];
				}
				else seenY.Add(currY, step);
			}

			var currZ = (
				planets[0].Z, planets[0].VZ,
				planets[1].Z, planets[1].VZ,
				planets[2].Z, planets[2].VZ,
				planets[3].Z, planets[3].VZ);
			if (loopZ == 0) {
				if (seenZ.ContainsKey(currZ)) {
					Console.WriteLine($"Seen Z @step {step} {currZ} first seen {seenZ[currZ]}");
					loopZ = step - seenZ[currZ];
				}
				else seenZ.Add(currZ, step);
			}
			
			if (step == 1000) planets.Sum(p => p.Kinetic).Dump("Step 1000 kinetic");
		}
		
		planets.Dump("Final State");
		LCM(loopX, loopY, loopZ).Dump("steps to repeat");
	}
}

struct Planet {
	public int X;
	public int Y;
	public int Z;

	public int VX;
	public int VY;
	public int VZ;

	public int Kinetic => (Abs(X) + Abs(Y) + Abs(Z)) * (Abs(VX) + Abs(VY) + Abs(VZ));

	public Planet(string line) {
		VX = VY = VZ = 0;
		var parts = line.Split('=', ',', '>');
		X = int.Parse(parts[1]);
		Y = int.Parse(parts[3]);
		Z = int.Parse(parts[5]);
	}
}