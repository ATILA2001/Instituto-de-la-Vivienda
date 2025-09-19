function showLoadingAnimation() {
    // Verifica si el formulario es v�lido antes de mostrar la animaci�n
    if (typeof(Page_ClientValidate) === 'function' && !Page_ClientValidate()) {
        return false;
    }
    
    // Muestra la capa de animaci�n
    const loadingElement = document.getElementById('loading-animation');
    loadingElement.style.display = 'block';
    
    // Crea la secuencia de animaci�n
    const timeline = gsap.timeline();
    
    timeline.fromTo("#logo",
        { opacity: 0, scale: 0.8 },
        { opacity: 1, scale: 1, duration: 0.8, ease: "power3.out" }
    );

    timeline.to("#logo", {
        opacity: 0,
        duration: 0.6,
        ease: "power3.in"
    });

    timeline.fromTo("#logo-2",
        { opacity: 0, scale: 0.8 },
        { opacity: 1, scale: 1, duration: 0.8, ease: "power3.out" }
    );

    timeline.to("#logo-2", {
        opacity: 0,
        duration: 0.6,
        ease: "power3.in"
    });
    
    timeline.fromTo("#logo-3",
        { opacity: 0, scale: 0.8 },
        { opacity: 1, scale: 1, duration: 0.8, ease: "power3.out" }
    );
    
    timeline.to("#logo-3", {
        scale: 1.1,
        duration: 0.6,
        ease: "power1.inOut",
        repeat: -1,
        yoyo: true
    });
    
    return true;
}