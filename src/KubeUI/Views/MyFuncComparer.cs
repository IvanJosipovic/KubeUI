using k8s.Models;

namespace KubeUI.Views;

public readonly struct MyFuncComparer<TObj, TPtop> : IComparer
{
    private readonly Func<TObj, TPtop> _cmp;

    public MyFuncComparer(Func<TObj, TPtop> cmp)
    {
        _cmp = cmp;
    }

    public int Compare(object? x, object? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null && y != null)
        {
            return -1;
        }

        if (x != null && y == null)
        {
            return 1;
        }

        var srcProperty = _cmp.Invoke((TObj)x);
        var destProperty = _cmp.Invoke((TObj)y);

        if (srcProperty == null && destProperty == null)
        {
            return 0;
        }

        if (srcProperty == null && destProperty != null)
        {
            return -1;
        }

        if (srcProperty != null && destProperty == null)
        {
            return 1;
        }

        if (srcProperty is string src && destProperty is string dest)
        {
            return src.CompareTo(dest);
        }
        else if (srcProperty is int src2 && destProperty is int dest2)
        {
            return src2.CompareTo(dest2);
        }
        else if (srcProperty is long src3 && destProperty is long dest3)
        {
            return src3.CompareTo(dest3);
        }
        else if (srcProperty is DateTime src4 && destProperty is DateTime dest4)
        {
            return src4.CompareTo(dest4);
        }
        else if (srcProperty is bool src5 && destProperty is bool dest5)
        {
            return src5.CompareTo(dest5);
        }
        else if (srcProperty is decimal src6 && destProperty is decimal dest6)
        {
            return src6.CompareTo(dest6);
        }
        else if (srcProperty is IntOrString src7 && destProperty is IntOrString dest7)
        {
            return src7.Value.CompareTo(dest7.Value);
        }

        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        if (obj is MyFuncComparer<TObj, TPtop> other)
        {
            var cmpComparer = EqualityComparer<Func<TObj, TPtop>>.Default;
            return cmpComparer.Equals(_cmp, other._cmp);
        }

        return false;
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(MyFuncComparer<TObj, TPtop> left, MyFuncComparer<TObj, TPtop> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MyFuncComparer<TObj, TPtop> left, MyFuncComparer<TObj, TPtop> right)
    {
        return !(left == right);
    }
}
