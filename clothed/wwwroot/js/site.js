
function toggleOptions() {
    const optionsOverlay = document.getElementById("optionsOverlay");
    console.log("toggleOptions called"); 
    if (optionsOverlay.classList.contains("show")) {
        optionsOverlay.classList.remove("show");
    } else {
        optionsOverlay.classList.add("show");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const mainImageWrapper = document.querySelector(".main-image-wrapper");
    const images = [
        "/images/RL1.png",
        "/images/adidas1.png",
        "/images/bones1.png",
        "/images/hoodie1.png",
        "/images/pegador1.png",
        "/images/WEEK1.png"
    ];
    let currentIndex = 0;

    if (mainImageWrapper) {
        mainImageWrapper.addEventListener("wheel", (event) => {
            event.preventDefault();

            if (event.deltaY > 0) {
                currentIndex = (currentIndex + 1) % images.length;
            } else {
                currentIndex = (currentIndex - 1 + images.length) % images.length;
            }

            const mainImage = document.querySelector(".main-image");
            if (mainImage) {
                mainImage.src = images[currentIndex];
                console.log(`Current image index: ${currentIndex}, src: ${mainImage.src}`);
            }
        });
    }

    document.addEventListener("mousemove", function (e) {
        let mainImage = document.querySelector(".main-image");
        if (mainImage) {
            let x = (e.clientX / window.innerWidth) * 10 - 5;
            let y = (e.clientY / window.innerHeight) * 10 - 5;
            mainImage.style.transform = `translate(${x}px, ${y}px)`;
        }
    });
});
