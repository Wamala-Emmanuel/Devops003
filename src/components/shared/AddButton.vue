<template>
    <div class="col-lg-5 ibox-content float-e-margins text-right ibox-add-user">
        <button class="btn btn-info " id="new-user" ref="new-user"
                type="button"  @click="$emit('slideOut')" aria-label="Add new">
            Add {{button_label}}
        </button>

    </div>
</template>

<script>
    export default {
        name: "AddButton",
        props: {
            button_label: String,
            path: String,
            action: Function
        },
    }
</script>
