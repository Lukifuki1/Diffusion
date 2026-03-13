using System.Text;
using ExifLibrary;

namespace AuraFlow.Core.Helper;

public class MyTiffFile(MemoryStream stream, Encoding encoding) : TIFFFile(stream, encoding);
