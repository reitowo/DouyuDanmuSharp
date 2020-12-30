# [douyudm](https://github.com/flxxyz/douyudm) C# implementation

### Usage

1. Add this Project to your Visual Studio Solution. The project uses `.NET 5.0`, however you probably can port to `.Net Framework`, etc..

2. `using LivestreamDanmuku.Douyu;`

3. Instantiate `DanmukuClient`.

   ```c#
   DanmukuClient danmu = new DanmukuClient(74751);
   danmu.Run();
   ```

   If you only want to receive chatting, use:

   ```c#
   danmu.OnChatMessage += Danmu_OnChatMessage;
   ```

   I'm not yet implement other message types, however it is able to hook these via:

   ```c#
   danmu.Register(new Dictionary<string, MessageHandler> {
       {
           "uenter",
           (msg) => {
               Debug.WriteLine($"用户进入 {msg["nn"]}");
           }
       },
       {
           "loginres",
           (msg) => {
               Debug.WriteLine("登陆成功");
           }
       }
   });
   ```

   This will clear the current callback table, and set the new one, to register to existing table, use

   ```c#
   danmu.Register("uenter", (msg) =>
   {
       Debug.WriteLine($"用户进入 {msg["nn"]}");
   });
   ```

   The `msg` parameter is `Newtonsoft.Json.Linq.JObject`.

