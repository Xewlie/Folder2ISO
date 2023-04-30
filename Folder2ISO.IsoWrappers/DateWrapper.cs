using ISO9660.PrimitiveTypes;

namespace Folder2ISO.IsoWrappers;

internal class DateWrapper
{
    // Wrapper class for DateTime, AsciiDateRecord and BinaryDateRecord
    
    private DateTime m_date;
    public BinaryDateRecord? BinaryDateRecord { get; private set; }
    public AsciiDateRecord? AsciiDateRecord { get; private set; }
    
    public DateWrapper(DateTime date)
    {
        Date = date;
    }

    public DateTime Date
    {
        set
        {
            m_date = value;
            SetAsciiDateRecord(value);
            SetBinaryDateRecord(value);
        }
    }

    public DateWrapper(DateTime date, sbyte timeZone)
    {
        m_date = date;
        SetAsciiDateRecord(date, timeZone);
        SetBinaryDateRecord(date);
    }
    
    public DateWrapper(BinaryDateRecord? dateRecord)
    {
        BinaryDateRecord = dateRecord;
        if (dateRecord != null)
        {
            SetAsciiDateRecord(1900 + dateRecord.Year, dateRecord.Month, dateRecord.DayOfMonth, dateRecord.Hour, dateRecord.Minute, dateRecord.Second, 0, 8);
            
            m_date = new DateTime(1900 + dateRecord.Year, dateRecord.Month, dateRecord.DayOfMonth, dateRecord.Hour, dateRecord.Minute, dateRecord.Second);
        }
    }

    public DateWrapper(AsciiDateRecord? dateRecord)
    {
        AsciiDateRecord = dateRecord;
        var yearOffset = (byte)(Convert.ToInt32(IsoAlgorithm.ByteArrayToString(dateRecord?.Year)) - 1900);
        var month = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord?.Month));
        var dayOfMonth = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord?.DayOfMonth));
        var hour = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord?.Hour));
        var minute = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord?.Minute));
        var second = Convert.ToByte(IsoAlgorithm.ByteArrayToString(dateRecord?.Second));
        var millisecond = Convert.ToInt32(IsoAlgorithm.ByteArrayToString(dateRecord?.HundredthsOfSecond)) * 10;
        SetBinaryDateRecord(yearOffset, month, dayOfMonth, hour, minute, second);
        m_date = new DateTime(1900 + yearOffset, month, dayOfMonth, hour, minute, second, millisecond);
    }

    private void SetBinaryDateRecord(byte year, byte month, byte dayOfMonth, byte hour, byte minute, byte second)
    {
        BinaryDateRecord ??= new BinaryDateRecord();
        BinaryDateRecord.Year = year;
        BinaryDateRecord.Month = month;
        BinaryDateRecord.DayOfMonth = dayOfMonth;
        BinaryDateRecord.Hour = hour;
        BinaryDateRecord.Minute = minute;
        BinaryDateRecord.Second = second;
    }

    private void SetBinaryDateRecord(DateTime date)
    {
        SetBinaryDateRecord((byte)(date.Year - 1900), (byte)date.Month, (byte)date.Day, (byte)date.Hour, (byte)date.Minute, (byte)date.Second);
    }

    private void SetAsciiDateRecord(int year, int month, int dayOfMonth, int hour, int minute, int second,
        int hundredthsOfSecond, sbyte timeZone)
    {
        AsciiDateRecord ??= new AsciiDateRecord();
        var yearString = $"{year % 10000:D4}";
        var monthString = $"{month:D2}";
        var dayOfMonthString = $"{dayOfMonth:D2}";
        var hourString = $"{hour:D2}";
        var minuteString = $"{minute:D2}";
        var secondString = $"{second:D2}";
        var hundredthsOfSecondString = $"{hundredthsOfSecond:D2}";
        AsciiDateRecord.Year = IsoAlgorithm.StringToByteArray(yearString);
        AsciiDateRecord.Month = IsoAlgorithm.StringToByteArray(monthString);
        AsciiDateRecord.DayOfMonth = IsoAlgorithm.StringToByteArray(dayOfMonthString);
        AsciiDateRecord.Hour = IsoAlgorithm.StringToByteArray(hourString);
        AsciiDateRecord.Minute = IsoAlgorithm.StringToByteArray(minuteString);
        AsciiDateRecord.Second = IsoAlgorithm.StringToByteArray(secondString);
        AsciiDateRecord.HundredthsOfSecond = IsoAlgorithm.StringToByteArray(hundredthsOfSecondString);
        AsciiDateRecord.TimeZone = timeZone;
    }

    private void SetAsciiDateRecord(DateTime date, sbyte timeZone = 8)
    {
        SetAsciiDateRecord(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond / 10, timeZone);
    }

    public void ResetAsciiDateRecord()
    {
        m_date = new DateTime(0, 0, 0, 0, 0, 0, 0);
        SetAsciiDateRecord(m_date);
        SetBinaryDateRecord(m_date);
    }
    public void WriteBinaryDateRecord(BinaryWriter writer)
    {
        if (BinaryDateRecord == null) return;
        writer.Write(new[]
        {
            BinaryDateRecord.Year, BinaryDateRecord.Month, BinaryDateRecord.DayOfMonth, BinaryDateRecord.Hour,
            BinaryDateRecord.Minute, BinaryDateRecord.Second
        });
    }

    public void WriteAsciiDateRecord(BinaryWriter writer)
    {
        if (AsciiDateRecord == null)
        {
            return;
        }
        
        writer.Write(AsciiDateRecord.Year ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.Month ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.DayOfMonth ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.Hour ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.Minute ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.Second ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.HundredthsOfSecond ?? Array.Empty<byte>());
        writer.Write(AsciiDateRecord.TimeZone);
    }
}