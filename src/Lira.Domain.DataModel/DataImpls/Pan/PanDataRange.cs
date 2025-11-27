using System.Diagnostics.CodeAnalysis;
using ArgValidation;
using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.DataModel.DataImpls.Pan;

public class PanDataRange : DataRange<string>
{
    private readonly Interval<int> _interval;
    private readonly IReadOnlyList<int[]> _bins;
    private readonly IReadOnlyList<string> _binsStr;
    private const int PanLength = 16;
    private readonly int _binLength;
    private const int CountRangeDigits = 6;

    public PanDataRange(DataName name, Interval<int> interval, int[] bins, string? description) : base(name, format: null, description)
    {
        Arg.Validate(bins, nameof(bins))
            .NotNull()
            .NotEmpty()
            .FailedIf(bins.Any(bin => bin < 1), $"Bins must be at least 1. Current: {string.Join(", ", bins)}");

        Arg.Validate(interval, nameof(interval))
            .NotNull()
            .FailedIf(interval.From < 0, $"The range from ({interval.From}) must be non-negative")
            .FailedIf(interval.To > 999_999, $"The range to ({interval.To}) must be less than 999_999");

        var binStrs = bins.Select(x => x.ToString()).ToList();
        _binLength = binStrs.Max(bin => bin.Length);


        const int countCheckDigit = 1;

        int maxBinLength = PanLength - countCheckDigit - CountRangeDigits;
        if(_binLength > maxBinLength)
            throw new Exception("Maximum bins length is greater than the maximum bin length: " + maxBinLength);

        var bns = new List<int[]>(bins.Length);
        foreach (var bin in binStrs)
        {
            // _bins.Add(bin.Select(c => byte.Parse(c.ToString())).ToArray());
            bns.Add(bin.PadRight(_binLength, '0').Select(c => int.Parse(c.ToString())).ToArray());
        }

        _bins = bns;
        _binsStr = binStrs;
        _interval = interval;
    }

    public override string Next()
    {
        var digits = new List<int>();
        digits.AddRange(_bins[Random.Shared.Next(_bins.Count)]);

        var rangeValue = Random.Shared.Next(_interval).ToString().PadLeft(CountRangeDigits, '0');

        foreach (var digit in rangeValue)
        {
            digits.Add(int.Parse(digit.ToString()));
        }

        while (digits.Count < PanLength - 1)
        {
            digits.Add(Random.Shared.Next(10));
        }

        digits.Add(CheckDigit(digits));
        return string.Concat(digits);
    }

    public override bool TryParse(string str, [MaybeNullWhen(false)] out string value)
    {
        value = null;

        if (str.Length != PanLength)
            return false;

        if (str.Any(c => !char.IsDigit(c)))
            return false;

        value = str;
        return true;
    }

    public override bool IsBelong(string value)
    {
        if (!_binsStr.Any(value.StartsWith))
            return false;

        var checkDigit = CheckDigit(value.Select(c => int.Parse(c.ToString())).Take(PanLength - 1).ToArray());
        if (checkDigit != int.Parse(value.Last().ToString()))
            return false;

        string s = value[_binLength..(_binLength + CountRangeDigits)];
        var rangeValue = int.Parse(s);
        return _interval.InRange(rangeValue);
    }

    static readonly int[] Results = [0, 2, 4, 6, 8, 1, 3, 5, 7, 9];

    /// <summary>
    /// For a list of digits, compute the ending checkdigit
    /// </summary>
    /// <param name="digits">The list of digits for which to compute the check digit</param>
    /// <returns>the check digit</returns>
    private static int CheckDigit(IReadOnlyList<int> digits)
    {
        var i = 0;
        var lengthMod = digits.Count%2;
        return (digits.Sum(d => i++ % 2 == lengthMod ? d : Results[d]) * 9) % 10;
    }
}
