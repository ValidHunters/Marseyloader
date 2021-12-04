using System;
using System.Data;
using Dapper;

namespace SS14.Launcher.Models.Data;

public sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString();
    }

    public override Guid Parse(object value)
    {
        return Guid.Parse((string) value);
    }
}

public sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("O");
    }

    public override DateTimeOffset Parse(object value)
    {
        return DateTimeOffset.Parse((string) value);
    }
}
