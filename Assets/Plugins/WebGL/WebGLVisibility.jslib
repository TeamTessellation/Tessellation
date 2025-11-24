mergeInto(LibraryManager.library, {
  RegisterVisibilityChangeCallback: function (goNamePtr) {
    var goName = UTF8ToString(goNamePtr);

    if (typeof document === "undefined") {
      console.log("[WebGLVisibility] document is undefined, callback not registered.");
      return;
    }

    var handler = function () {
      var state = document.visibilityState; // "visible", "hidden", ...
      SendMessage(goName, "OnPageVisibilityChanged", state);
    };

    document.addEventListener("visibilitychange", handler);

    // 초기 상태도 한 번 보내기
    handler();
  }
});
