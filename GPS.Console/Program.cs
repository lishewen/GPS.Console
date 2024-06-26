using System.IO.Ports;
#if DEBUG
const string PortName = "COM4";
#else
const string PortName = "/dev/ttyUSB0";
#endif
string utctime = "";
string lat = "";
string ulat = "";
string lon = "";
string ulon = "";
string numSv = "";
string msl = "";
string cogt = "";
string cogm = "";
string sog = "";
string kph = "";
int gps_t = 0;

SerialPort ser = new(PortName, 9600);

ser.Open();

if (ser.IsOpen)
{
    Console.WriteLine("GPS Serial Opened! Baudrate=9600");
}
else
{
    Console.WriteLine("GPS Serial Open Failed!");
}

try
{
    while (true)
    {
        if (GPS_read() == 1)
        {
            Console.WriteLine("*********************");
            Console.WriteLine("UTC Time:" + utctime);
            Console.WriteLine("Latitude:" + lat + ulat);
            Console.WriteLine("Longitude:" + lon + ulon);
            Console.WriteLine("Number of satellites:" + numSv);
            Console.WriteLine("Altitude:" + msl);
            Console.WriteLine("True north heading:" + cogt + "°");
            Console.WriteLine("Magnetic north heading:" + cogm + "°");
            Console.WriteLine("Ground speed:" + sog + "Kn");
            Console.WriteLine("Ground speed:" + kph + "Km/h");
            Console.WriteLine("*********************");
        }
    }
}
catch (Exception)
{
    ser.Close();
    Console.WriteLine("GPS serial Close!");
}

static double Convert_to_degrees(string in_data1, string in_data2)
{
    int len_data1 = in_data1.Length;
    string str_data2 = $"{int.Parse(in_data2):00000}";
    int temp_data = int.Parse(in_data1);
    int symbol = 1;
    if (temp_data < 0)
    {
        symbol = -1;
    }
    int degree = temp_data / 100;
    string str_decimal = string.Concat(in_data1.AsSpan(len_data1 - 2), str_data2);
    double f_degree = int.Parse(str_decimal) / 60.0 / 100000.0;
    double result;
    if (symbol > 0)
    {
        result = degree + f_degree;
    }
    else
    {
        result = degree - f_degree;
    }
    return result;
}

int GPS_read()
{
    string str = ser.ReadLine();
    if (str.StartsWith("$GNGGA"))
    {
        string[] GGA_g = str.Split(',');
        if (GGA_g.Length < 14 || string.IsNullOrWhiteSpace(GGA_g[2]))
        {
            Console.WriteLine("GPS no found");
            gps_t = 0;
            return 0;
        }
        else
        {
            // Console.WriteLine(str);
            utctime = GGA_g[1];
            string[] d2 = GGA_g[2].Split(".");
            lat = $"{Convert_to_degrees(d2[0], d2[1]):0.00000000}";
            ulat = GGA_g[3];
            string[] d4 = GGA_g[4].Split(".");
            lon = $"{Convert_to_degrees(d4[0], d4[1]):0.00000000}";
            ulon = GGA_g[5];
            numSv = GGA_g[7];
            msl = GGA_g[9] + GGA_g[12];
            gps_t = 1;
            return 1;
        }
    }
    else if (str.StartsWith("$GNVTG"))
    {
        if (gps_t == 1)
        {
            // Console.WriteLine(str);
            string[] VTG_g = str.Split(',');
            cogt = VTG_g[1] + "T";
            if (VTG_g[3] == "M")
            {
                cogm = "0.00";
                sog = VTG_g[4];
                kph = VTG_g[6];
            }
            else if (VTG_g[3] != "M")
            {
                cogm = VTG_g[3];
                sog = VTG_g[5];
                kph = VTG_g[7];
            }
        }
    }
    return 0;
}