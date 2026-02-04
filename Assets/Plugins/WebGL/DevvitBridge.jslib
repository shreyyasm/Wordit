mergeInto(LibraryManager.library, {
  OpenPostWithUrl: function (urlPtr) {
    const url = UTF8ToString(urlPtr);
    console.log('[JSLIB] OpenPostWithUrl:', url);

    if (window.navigateToPost) {
      window.navigateToPost(url);
    } else {
      console.error('[JSLIB] navigateToPost not found');
    }
  }
});

