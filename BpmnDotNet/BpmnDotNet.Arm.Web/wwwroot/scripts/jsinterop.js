function FocusElement(id) {
    const element = document.getElementById(id);
    if (element == null)
        return
    element.focus({preventScroll: true});
}

async function LoadImg(idCanvas, imgBase64, width, height) {
    if (imgBase64 == null || width <= 0 || height <= 0) {
        console.log("Fail input data");
        return;
    }
    const img = new Image();
    await new Promise(r => {
        img.onload = r;
        img.src = imgBase64;
    });

    // console.log("chartCanvas  ",imgBase64);

    const canvas = document.getElementById(idCanvas);
    if (canvas == null) {
        console.log("canvas null", idCanvas);
        return -1;
    }


    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, width, height);

    ctx.drawImage(img, 0, 0, width, height);

}

window.registerViewportChangeCallback = (dotnetHelper) => {
    window.addEventListener('load', () => {
        dotnetHelper.invokeMethodAsync('OnResize', window.innerWidth, window.innerHeight);
    });
    window.addEventListener('resize', () => {
        dotnetHelper.invokeMethodAsync('OnResize', window.innerWidth, window.innerHeight);
    });
}

function GetBrowseSize(idElement) {

    const element = document.getElementById(idElement);
    const positionInfo = element.getBoundingClientRect();
    const height = positionInfo.height;
    const width = positionInfo.width;

    // console.log("positionInfo.height  ",positionInfo.height);
    // console.log("positionInfo.width  ",positionInfo.width);
    // console.log("positionInfo.x  ",positionInfo.x);
    // console.log("positionInfo.y  ",positionInfo.y);

    return {
        Width: width,
        Height: height,
        X: positionInfo.x,
        Y: positionInfo.y,
    };
}

function SetBrowseSize(idElement, width, height) {

    const element = document.getElementById(idElement);
    element.width = width;
    element.height = height;

    // console.log("width  ",width);
    // console.log("element.width  ",element.width);

}


function FocusElement(id) {
    const element = document.getElementById(id);
    if (element == null)
        return
    element.focus({preventScroll: true});
}


