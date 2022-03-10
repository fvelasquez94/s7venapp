var stripe = Stripe('pk_live_51JaSbuLIXevd0ZypmZENKQtmRwjdUk89fyQljpmxCnbwXRbWCYmWstP0VMH1Wu7CPk5tS7TfnRSeEMoOjlfwLhkq00wqo0QMdH');

var elements = stripe.elements();

// Set up Stripe.js and Elements to use in checkout form
var style = {
    base: {
        color: "#32325d",
        fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
        fontSmoothing: "antialiased",
        fontSize: "16px",
        "::placeholder": {
            color: "#aab7c4"
        }
    },
    invalid: {
        color: "#fa755a",
        iconColor: "#fa755a"
    },
};

var cardElement = elements.create('card', { style: style });
cardElement.mount('#card-element');

var form = document.getElementById('payment-form');

form.addEventListener('submit', function (event) {
    // We don't want to let default form submission happen here,
    // which would refresh the page.
    event.preventDefault();

    stripe.createPaymentMethod({
        type: 'card',
        card: cardElement,
        billing_details: {
            // Include any additional collected billing details.
            name: 'Jenny Rosen',
        },
    }).then(stripePaymentMethodHandler);
});

function stripePaymentMethodHandler(result) {
    if (result.error) {
        // Show error in payment form
    } else {
        // Otherwise send paymentMethod.id to your server (see Step 4)
        fetch('/pay', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                payment_method_id: result.paymentMethod.id,
            })
        }).then(function (result) {
            // Handle server response (see Step 4)
            result.json().then(function (json) {
                handleServerResponse(json);
            })
        });
    }
}