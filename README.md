# Vision4GP

Library that works with vision file system ( [Microfocus Acucobol-GT](https://www.microfocus.com/it-it/products/acucobol-gt/overview) ).

## Requirements

Vision library works with Microfocus Acucobol-GT runtime. 
For work with it, you need:
- A specific license from Microfocus to use their V6 library ( libvision64.so for Linux, avision6.dll for Windows)
- FILE_PREFIX environment (where to find the vision file)
- XFD_DIRECTORY environment (where to find xfd files)
- VISION_LICENSE_FILE environment (full path ot the Microfocus license file)


## How to use the library

Suppose to have the customer vision file that has this record definition:

    01 CUST-RECORD.
       03 CUST-KEY.
          05 CUST-CODE         PIC 9(05).
       03 CUST-DATA.
          05 CUST-NAME         PIC X(40).
          05 CUST-ALIAS        PIC X(40).
          05 CUST-ADDRESS      PIC X(40).

### Read all records

To read all records of a file, we have to write this code:

    var fileSystem = VisionFileSystem.GetInstance();
    using (var file = fileSystem.GetVisionFile("customer"))
    {
       file.Open(FileOpenMode.Input);
       if (file.Start())
       {
          IVisionRecord record;
          while (true)
          {
              record = file.ReadNext();
              if (record == null) break;

              Console.WriteLine($"Code: {record.GetValue("CUST_CODE")}");
              Console.WriteLine($"Name: {record.GetValue("CUST_NAME")}");
              Console.WriteLine($"Alias: {record.GetValue("CUST_ALIAS")}");
          }
       }
       file.Close();
    }

### Read a single record

To read the record with CUST-CODE equal 5, we have to write this code:

    var fileSystem = VisionFileSystem.GetInstance();
    using (var file = fileSystem.GetVisionFile("customer"))
    {
       var record = file.GetNewRecord();
       record.SetValue("CUST_CODE", 5); // can use also the SetInt method
       file.Open(FileOpenMode.Input);
       record = file.ReadLock(record);
       if (record != null)
       {
           Console.WriteLine($"Code: {record.GetValue("CUST_CODE")}");
           Console.WriteLine($"Name: {record.GetValue("CUST_NAME")}");
           Console.WriteLine($"Alias: {record.GetValue("CUST_ALIAS")}");
       }
       file.Close();
    }

### Update a record

To change the name of the customer with CUST-CODE equal 5, we have to write this code:

    var fileSystem = VisionFileSystem.GetInstance();
    using (var file = fileSystem.GetVisionFile("customer"))
    {
       var record = file.GetNewRecord();
       record.SetValue("CUST_CODE", 5); // can use also the SetInt method
       file.Open(FileOpenMode.InputOutput);
       record = file.ReadLock(record);
       if (record != null)
       {
          record.SetValue("CUST_NAME", "New customer name");
          file.Rewrite(record);
       }
       file.Close();
    }


### Delete a record

To delete the record with CUST-CODE equal 5, we have to write this code:

    var fileSystem = VisionFileSystem.GetInstance();
    using (var file = fileSystem.GetVisionFile("customer"))
    {
       var record = file.GetNewRecord();
       record.SetValue("CUST_CODE", 5); // can use also the SetInt method
       file.Open(FileOpenMode.InputOutput);
       record = file.ReadLock(record);
       if (record != null)
       {
          file.Delete(record);
       }
       file.Close();
    }
