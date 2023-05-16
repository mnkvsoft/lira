using SimpleMockServer.Common.Exceptions;

namespace SimpleMockServer.Domain;

public interface IRuleMatchWeight : IComparable<IRuleMatchWeight>
{
    
}

class RuleMatchWeight : IRuleMatchWeight
{
    private readonly int _method;
    private readonly int _path;
    private readonly int _query;
    private readonly int _body;
    private readonly int _headers;

    public RuleMatchWeight(int method, int path, int query, int body, int headers)
    {
        _method = method;
        _path = path;
        _query = query;
        _body = body;
        _headers = headers;
    }

    public int CompareTo(IRuleMatchWeight? other)
    {
        if (other == null)
            return 1;
        
        if (other is not RuleMatchWeight otherWeight)
            throw new UnsupportedInstanceType(other);

        int compareResult = this._method.CompareTo(otherWeight._method);
        if (compareResult != 0)
            return compareResult;
        
        compareResult = this._path.CompareTo(otherWeight._path);
        if (compareResult != 0)
            return compareResult;

        int sum = _query + _body + _headers;
        int sumOther = otherWeight._query + otherWeight._body + otherWeight._headers;

        return sum.CompareTo(sumOther);
    }
}
