Imports System
Imports System.Text
'
' * http://www.ietf.org/rfc/rfc1876.txt
' * 
'2. RDATA Format
'
' MSB LSB
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 0| VERSION | SIZE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 2| HORIZ PRE | VERT PRE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 4| LATITUDE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 6| LATITUDE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 8| LONGITUDE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 10| LONGITUDE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 12| ALTITUDE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' 14| ALTITUDE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'VERSION Version number of the representation. This must be zero.
' Implementations are required to check this field and make
' no assumptions about the format of unrecognized versions.
'
'SIZE The diameter of a sphere enclosing the described entity, in
' centimeters, expressed as a pair of four-bit unsigned
' integers, each ranging from zero to nine, with the most
' significant four bits representing the base and the second
' number representing the power of ten by which to multiply
' the base. This allows sizes from 0e0 (<1cm) to 9e9
' (90,000km) to be expressed. This representation was chosen
' such that the hexadecimal representation can be read by
' eye; 0x15 = 1e5. Four-bit values greater than 9 are
' undefined, as are values with a base of zero and a non-zero
' exponent.
'
' Since 20000000m (represented by the value 0x29) is greater
' than the equatorial diameter of the WGS 84 ellipsoid
' (12756274m), it is therefore suitable for use as a
' "worldwide" size.
'
'HORIZ PRE The horizontal precision of the data, in centimeters,
' expressed using the same representation as SIZE. This is
' the diameter of the horizontal "circle of error", rather
' than a "plus or minus" value. (This was chosen to match
' the interpretation of SIZE; to get a "plus or minus" value,
' divide by 2.)
'
'VERT PRE The vertical precision of the data, in centimeters,
' expressed using the sane representation as for SIZE. This
' is the total potential vertical error, rather than a "plus
' or minus" value. (This was chosen to match the
' interpretation of SIZE; to get a "plus or minus" value,
' divide by 2.) Note that if altitude above or below sea
' level is used as an approximation for altitude relative to
' the [WGS 84] ellipsoid, the precision value should be
' adjusted.
'
'LATITUDE The latitude of the center of the sphere described by the
' SIZE field, expressed as a 32-bit integer, most significant
' octet first (network standard byte order), in thousandths
' of a second of arc. 2^31 represents the equator; numbers
' above that are north latitude.
'
'LONGITUDE The longitude of the center of the sphere described by the
' SIZE field, expressed as a 32-bit integer, most significant
' octet first (network standard byte order), in thousandths
' of a second of arc, rounded away from the prime meridian.
' 2^31 represents the prime meridian; numbers above that are
' east longitude.
'
'ALTITUDE The altitude of the center of the sphere described by the
' SIZE field, expressed as a 32-bit integer, most significant
' octet first (network standard byte order), in centimeters,
' from a base of 100,000m below the [WGS 84] reference
' spheroid used by GPS (semimajor axis a=6378137.0,
' reciprocal flattening rf=298.257223563). Altitude above
' (or below) sea level may be used as an approximation of
' altitude relative to the the [WGS 84] spheroid, though due
' to the Earth's surface not being a perfect spheroid, there
' will be differences. (For example, the geoid (which sea
' level approximates) for the continental US ranges from 10
' meters to 50 meters below the [WGS 84] spheroid.
' Adjustments to ALTITUDE and/or VERT PRE will be necessary
' in most cases. The Defense Mapping Agency publishes geoid
' height values relative to the [WGS 84] ellipsoid.
'
' 


Namespace Dns

    Public Class RecordLOC
        Inherits Record
        Public VERSION As Byte
        Public SIZE As Byte
        Public HORIZPRE As Byte
        Public VERTPRE As Byte
        Public LATITUDE As UInt32
        Public LONGITUDE As UInt32
        Public ALTITUDE As UInt32

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Private Function SizeToString(ByVal s As Byte) As String
            Dim strUnit As String = "cm"
            Dim intBase As Integer = s >> 4
            Dim intPow As Integer = s And &HF
            If intPow >= 2 Then
                intPow -= 2
                strUnit = "m"
            End If
            '
            ' if (intPow >= 3)
            ' {
            ' intPow -= 3;
            ' strUnit = "km";
            ' }
            ' 

            Dim sb As New StringBuilder()
            sb.AppendFormat("{0}", intBase)
            While intPow > 0
                sb.Append("0"c)
                intPow -= 1
            End While
            sb.Append(strUnit)
            Return sb.ToString()
        End Function

        Private Function LonToTime(ByVal r As UInt32) As String
            Dim Mid As UInt32 = 2147483648
            ' 2^31
            Dim Dir As Char = "E"c
            If r > Mid Then
                Dir = "W"c
                r -= Mid
            End If
            Dim h As Double = r / (360000.0R * 10.0R)
            Dim m As Double = 60.0R * (h - CInt(h))
            Dim s As Double = 60.0R * (m - CInt(m))
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0} {1} {2:0.000} {3}", CInt(h), CInt(m), s, Dir)
        End Function

        Private Shared Function ToTime(ByVal r As UInt32, ByVal Below As Char, ByVal Above As Char) As String
            Dim Mid As UInt32 = 2147483648
            ' 2^31
            Dim Dir As Char = "?"c
            If r > Mid Then
                Dir = Above
                r -= Mid
            Else
                Dir = Below
                r = Mid - r
            End If
            Dim h As Double = r / (360000.0R * 10.0R)
            Dim m As Double = 60.0R * (h - CInt(h))
            Dim s As Double = 60.0R * (m - CInt(m))
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0} {1} {2:0.000} {3}", CInt(h), CInt(m), s, Dir)
        End Function

        Private Shared Function ToAlt(ByVal a As UInt32) As String
            Dim alt As Double = (a / 100.0R) - 100000.0R
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0:0.00}m", alt)
        End Function

        Public Sub New(ByVal rr As RecordReader)
            VERSION = rr.ReadByte()
            ' must be 0!
            SIZE = rr.ReadByte()
            HORIZPRE = rr.ReadByte()
            VERTPRE = rr.ReadByte()
            LATITUDE = rr.ReadUInt32()
            LONGITUDE = rr.ReadUInt32()
            ALTITUDE = rr.ReadUInt32()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5}", ToTime(LATITUDE, "S"c, "N"c), ToTime(LONGITUDE, "W"c, "E"c), ToAlt(ALTITUDE), SizeToString(SIZE), SizeToString(HORIZPRE), _
            SizeToString(VERTPRE))
        End Function

    End Class
End Namespace
