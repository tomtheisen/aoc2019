<Query Kind="Program">
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
</Query>

#load ".\helpers.linq"

void Main() {
}

public class IntCodeMachine {
	public event Action<BigInteger>? Outputting;
	public event Action<BigInteger>? UnhandledInput;

	public BigInteger[] InitialState { get; }
	public bool Terminated { get; private set; } = true;
	public string? Name { get; private set; }
	public Func<BigInteger>? NextInput { private get; set; } = null;

	private Queue<BigInteger> Input = new Queue<BigInteger>();
	private DefaultDictionary<BigInteger, BigInteger> Memory = new DefaultDictionary<BigInteger, BigInteger>(_ => 0);
	private BigInteger IP;
	private BigInteger RelativeBase;

	public IntCodeMachine() : this(GetAocBigIntegers()) { }

	public IntCodeMachine(IReadOnlyList<BigInteger> initialState, string? name = default) {
		InitialState = initialState.ToArray();
		Name = name;
		Reset();
	}

	public void Reset() {
		RelativeBase = IP = 0;
		Input.Clear();
		Memory.Clear();
		for (int i = 0; i < InitialState.Length; i++) if (InitialState[i] != 0) Memory[i] = InitialState[i];
		Terminated = false;
	}

	public void TakeInput(BigInteger num) {
		if (Terminated) UnhandledInput?.Invoke(num);
		else Input.Enqueue(num);
	}

	public IReadOnlyDictionary<BigInteger, BigInteger> Run(BigInteger? noun = null, BigInteger? verb = null) {
		if (noun.HasValue) Memory[1] = noun.Value;
		if (verb.HasValue) Memory[2] = verb.Value;
		while (Tick()) { }
		return Memory;
	}

	/// <summary>returns true iff work was done</summary>
	public bool Tick() {
		if (Terminated) return false;

		BigInteger GetResolvedAddress(int idx) => 
            (int)(Memory[IP] / BigInteger.Pow(10, idx + 1) % 10) switch { 
                0 => Memory[IP + idx], 
                1 => IP + idx, 
                2 => RelativeBase + Memory[IP + idx],
                _ => throw new Exception($"Opcode modifier for {Memory[IP]} out of range"),
            };
            
        BigInteger Read(int idx) => Memory[GetResolvedAddress(idx)];
		void Write(int idx, BigInteger value) => Memory[GetResolvedAddress(idx)] = value;
		
		switch ((int)(Memory[IP] % 100)) {
			case 1: // add
				Write(3, Read(1) + Read(2));
				IP += 4;
				return true;
			case 2: // mul
				Write(3, Read(1) * Read(2));
				IP += 4;
				return true;
			case 3: // in
				if (Input.Count > 0) {
					Write(1, Input.Dequeue());
					IP += 2;
					return true;
				}
				if (NextInput != null) {
					Write(1, NextInput());
					IP += 2;
					return true;
				}
				return false; // work later
			case 4: // out
				var output = Read(1);
				Outputting?.Invoke(output);
				IP += 2;
				return true;
			case 5: // bnz
				if (Read(1) != 0) IP = Read(2);
				else IP += 3;
				return true;
			case 6: // bez
				if (Read(1) == 0) IP = Read(2);
				else IP += 3;
				return true;
			case 7: // lt
				Write(3, Read(1) < Read(2) ? 1 : 0);
				IP += 4;
				return true;
			case 8: // eq
				Write(3, Read(1) == Read(2) ? 1 : 0);
				IP += 4;
				return true;
			case 9: // set rel
				RelativeBase += Read(1);
				IP += 2;
				return true;
			case 99: // halt
				Terminated = true;
				if (UnhandledInput == null) Input.Clear();
				else while (Input.Any()) UnhandledInput?.Invoke(Input.Dequeue());
				return false; // done

			default: throw new ArgumentOutOfRangeException("opcode");
		}
	}
}

public class IntCodeCluster {
	public IReadOnlyList<IntCodeMachine> Machines { get; private set; }

	public IntCodeCluster(params IntCodeMachine[] machines) => Machines = machines;

	public IntCodeCluster(IEnumerable<IntCodeMachine> machines) => Machines = machines.ToArray();

	public void Reset() {
		foreach (var machine in Machines) machine.Reset();
	}

	public bool Tick()
		=> Machines.Aggregate(false, (didwork, machine) => didwork |= machine.Tick());

	public void Run() {
		while (Tick()) { }
	}

	public IntCodeMachine this[Index index] => Machines[index];

	public int Count => Machines.Count;
}

public class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey: notnull 
{
    private Func<TKey, TValue> DefaultFactory;
    private IDictionary<TKey, TValue> State = new Dictionary<TKey, TValue>();
    
    public DefaultDictionary(Func<TKey, TValue> defaultFactory) => DefaultFactory = defaultFactory;
    
    public TValue this[TKey key] { 
        get => State.TryGetValue(key, out var result) ? result : State[key] = DefaultFactory(key);
        set => State[key] = value;
    }

    public ICollection<TKey> Keys => State.Keys;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    public ICollection<TValue> Values => State.Values;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
    public int Count => State.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value) => State.Add(key, value);
    public void Add(KeyValuePair<TKey, TValue> item) => State.Add(item);
    public void Clear() => State.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item) => State.ContainsKey(item.Key)
        ? object.Equals(State[item.Key], item.Value)
        : object.Equals(DefaultFactory(item.Key), item.Value);

    public bool ContainsKey(TKey key) => true;
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => State.CopyTo(array, arrayIndex);

    public bool Remove(TKey key) => State.Remove(key);
    public bool Remove(KeyValuePair<TKey, TValue> item) => State.Remove(item);
    
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        value = State.TryGetValue(key, out var result) ? result : State[key] = DefaultFactory(key);
        return true;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => State.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
}