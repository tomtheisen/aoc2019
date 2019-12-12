<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

BigInteger LCM(params BigInteger[] values) 
	=> values.Aggregate(BigInteger.One, 
		(a, b) => a * b / BigInteger.GreatestCommonDivisor(a, b));

void Main() {
	var planets = GetAocLines().Select(line => new Planet(line)).ToArray().Dump("Initial");
	var initial = planets[..];
	long loopX = 0, loopY = 0, loopZ = 0;

	for (long step = 1; loopX * loopY * loopZ == 0; step++) {
		for (int i = 0; i < planets.Length; i++) {
			for (int j = 0; j < planets.Length; j++) {
				planets[i].VX += Sign(planets[j].X - planets[i].X);
				planets[i].VY += Sign(planets[j].Y - planets[i].Y);
				planets[i].VZ += Sign(planets[j].Z - planets[i].Z);
			}
		}

		for (int i = 0; i < planets.Length; i++) {
			planets[i].X += planets[i].VX;
			planets[i].Y += planets[i].VY;
			planets[i].Z += planets[i].VZ;
		}
		
		if (loopX == 0 && planets.Zip(initial, (p, i) => p.X == i.X && p.VX == i.VX).All(eq => eq)) loopX = step;
		if (loopY == 0 && planets.Zip(initial, (p, i) => p.Y == i.Y && p.VY == i.VY).All(eq => eq)) loopY = step;
		if (loopZ == 0 && planets.Zip(initial, (p, i) => p.Z == i.Z && p.VZ == i.VZ).All(eq => eq)) loopZ = step;

		if (step == 1000) planets.Sum(p => p.Kinetic).Dump("Step 1000 kinetic");
	}
	
	LCM(loopX, loopY, loopZ).Dump("steps to repeat");
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