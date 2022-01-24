<template>
    <div class="form-group text-right">
        <button class="btn btn-w-m btn-primary mt-5" id="submit-user" type="button">{{title}}</button>
    </div>
</template>

<script>
    export default {
        name: "SubmitButton",
        props: {
            title: String
        }
    }
</script>
