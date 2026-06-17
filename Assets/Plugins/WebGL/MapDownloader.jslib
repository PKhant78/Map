mergeInto(LibraryManager.library, {
  DownloadTextFile: function (filenamePtr, contentPtr) {
    var filename = UTF8ToString(filenamePtr);
    var content  = UTF8ToString(contentPtr);
    var blob = new Blob([content], { type: "application/json" });
    var url  = URL.createObjectURL(blob);
    var a    = document.createElement("a");
    a.href     = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }
});
