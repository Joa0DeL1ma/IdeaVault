// Minimal JS interop: the browser needs JS to save a generated file.
window.ideaVault = {
  download(fileName, content) {
    const url = URL.createObjectURL(new Blob([content], { type: "application/json" }));
    const link = Object.assign(document.createElement("a"), { href: url, download: fileName });
    link.click();
    URL.revokeObjectURL(url);
  },
};
