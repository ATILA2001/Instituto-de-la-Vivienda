// Global variable to track if animation is currently showing
var animationIsActive = false;

function showLoadingAnimation(validationGroup) {
    // Only proceed if this is the Login validation group or no group is specified
    if (validationGroup && validationGroup !== 'Login') {
        return true; // Skip animation for non-login actions
    }
    
    // Verifica si el formulario es válido antes de mostrar la animación
    if (typeof(Page_ClientValidate) === 'function') {
        if (!Page_ClientValidate(validationGroup)) {
            return false;
        }
    }
    
    // Muestra la capa de animación
    const loadingElement = document.getElementById('loading-animation');
    if (!loadingElement) return true; // Si no existe el elemento, continuamos
    
    // Prevent multiple animations from running
    if (animationIsActive) {
        return true;
    }
    
    animationIsActive = true;
    loadingElement.style.display = 'block';
    
    // Verificamos que GSAP esté disponible
    if (typeof gsap === 'undefined') {
        console.warn('GSAP library not loaded. Animation will not run.');
        return true;
    }
    
    // Detenemos cualquier animación existente
    gsap.killTweensOf("#logo");
    gsap.killTweensOf("#logo-2");
    gsap.killTweensOf("#logo-3");
    
    // Reseteamos el estado inicial
    gsap.set("#logo", { opacity: 0, scale: 0.8 });
    gsap.set("#logo-2", { opacity: 0, scale: 0.8 });
    gsap.set("#logo-3", { opacity: 0, scale: 0.8 });
    
    // Crea la secuencia de animación
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
    
    // Set a timeout to ensure the form submission happens even if animation gets stuck
    setTimeout(function() {
        // If we're still on the same page after 8 seconds, force reset animation state
        hideLoadingAnimation();
    }, 8000);
    
    return true;
}

// Handler for hiding the loading animation
function hideLoadingAnimation() {
    const loadingElement = document.getElementById('loading-animation');
    if (loadingElement) {
        loadingElement.style.display = 'none';
    }
    animationIsActive = false;
}

// Add event listener to hide animation when page is fully loaded
if (window.addEventListener) {
    window.addEventListener('load', function() {
        // Reset animation state on page load
        animationIsActive = false;
    });
} else if (window.attachEvent) {
    window.attachEvent('onload', function() {
        // Reset animation state on page load
        animationIsActive = false;
    });
}

// When the page is about to be unloaded (navigation away from page)
if (window.addEventListener) {
    window.addEventListener('beforeunload', function() {
        // Intentionally keep animation visible during page transition
    });
}