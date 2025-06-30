// snippet.using
using PubnubApi;

// snippet.end

public class FilesSample
{
    private static Pubnub pubnub;

    static void InitSample()
    {
        // snippet.init
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId("myUniqueUserId"))
        {
            SubscribeKey = "demo",
            PublishKey = "demo",
            Secure = true
        };

        Pubnub pubnub = new Pubnub(pnConfiguration);
        // snippet.end
    }
    
    public static async Task SendFileBasicUsage()
    {
        // snippet.send_file_basic_usage
        try
        {
            PNResult<PNFileUploadResult> fileUploadResponse = await pubnub.SendFile()
                .Channel("my_channel")
                .File("path/to/your/file/cat_picture.jpg")
                .Message("Look at this photo!")
                .CustomMessageType("file-message")
                .ExecuteAsync();

            PNFileUploadResult fileUploadResult = fileUploadResponse.Result;
            PNStatus fileUploadStatus = fileUploadResponse.Status;

            if (!fileUploadStatus.Error && fileUploadResult != null)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileUploadResult));
            }
            else
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileUploadStatus));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request cannot be executed due to error: {ex.Message}");
        }
        // snippet.end
    }

    public static async Task ListFilesBasicUsage()
    {
        // snippet.list_files_basic_usage
        PNResult<PNListFilesResult> listFilesResponse = await pubnub.ListFiles()
            .Channel("my_channel")
            .ExecuteAsync();
        PNListFilesResult listFilesResult = listFilesResponse.Result;
        PNStatus listFilesStatus = listFilesResponse.Status;
        if (!listFilesStatus.Error && listFilesResult != null)
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(listFilesResult));
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(listFilesStatus));
        }
        // snippet.end
    }

    public static async Task GetFileUrlBasicUsage()
    {
        // snippet.get_file_url_basic_usage
        PNResult<PNFileUrlResult> getFileUrlResponse = await pubnub.GetFileUrl()
            .Channel("my_channel")
            .FileId("d9515cb7-48a7-41a4-9284-f4bf331bc770")
            .FileName("cat_picture.jpg")
            .ExecuteAsync();
        PNFileUrlResult getFileUrlResult = getFileUrlResponse.Result;
        PNStatus getFileUrlStatus = getFileUrlResponse.Status;
        if (!getFileUrlStatus.Error && getFileUrlResult != null)
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getFileUrlResult));
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(getFileUrlStatus));
        }
        // snippet.end
    }

    public static async Task DownloadFileBasicUsage(string downloadUrlFileName)
    {
        // snippet.download_file_basic_usage
        PNResult<PNDownloadFileResult> fileDownloadResponse = await pubnub.DownloadFile()
            .Channel("my_channel")
            .FileId("d9515cb7-48a7-41a4-9284-f4bf331bc770")
            .FileName("cat_picture.jpg")
            .ExecuteAsync();
        PNDownloadFileResult fileDownloadResult = fileDownloadResponse.Result;
        PNStatus fileDownloadStatus = fileDownloadResponse.Status;
        if (!fileDownloadStatus.Error && fileDownloadResult != null)
        {
            fileDownloadResult.SaveFileToLocal(downloadUrlFileName); //saves to bin folder if no path is provided
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileDownloadResult.FileName));
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(fileDownloadStatus));
        }
        // snippet.end
    }

    public static async Task DeleteFileBasicUsage()
    {
        // snippet.delete_file_basic_usage
        PNResult<PNDeleteFileResult> deleteFileResponse = await pubnub.DeleteFile()
            .Channel("my_channel")
            .FileId("d9515cb7-48a7-41a4-9284-f4bf331bc770")
            .FileName("cat_picture.jpg")
            .ExecuteAsync();
        PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
        PNStatus deleteFileStatus = deleteFileResponse.Status;
        if (!deleteFileStatus.Error && deleteFileResult != null)
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(deleteFileResult));
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(deleteFileStatus));
        }
        // snippet.end
    }

    public static async Task PublishFileMessageBasicUsage()
    {
        // snippet.publish_file_message_basic_usage
        PNResult<PNPublishFileMessageResult> publishFileMsgResponse = await pubnub.PublishFileMessage()
            .Channel("my_channel")
            .FileId("d9515cb7-48a7-41a4-9284-f4bf331bc770")
            .FileName("cat_picture.jpg") //checks the bin folder if no path is provided
            .Message("This is a sample message")
            .CustomMessageType("file-message")
            .ExecuteAsync();
        PNPublishFileMessageResult publishFileMsgResult = publishFileMsgResponse.Result;
        PNStatus publishFileMsgStatus = publishFileMsgResponse.Status;
        if (!publishFileMsgStatus.Error && publishFileMsgResult != null)
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(publishFileMsgResult));
        }
        else
        {
            Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(publishFileMsgStatus));
        }
        // snippet.end
    }
}