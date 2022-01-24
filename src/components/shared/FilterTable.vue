<template>
    <div class="col- m-b-xs">
        <v-select class="input-s-sm inline" transition="none" v-model="selected" @input="setSelected(selected)" :options="options" :clearable="false">

        </v-select>

    </div>
</template>

<script>
    import vSelect from 'vue-select'

    export default {
        name: "FilterTable",
        components: {
            vSelect,
        },
        data(){
            return {
               selected: 5,
               oldRows:[]
            }
        },
        props: {
            options: Array,

        },
        methods: {
            setSelected(value) {
                // this.oldRows = this.dataToFilter;
                this.$emit('filterData', value)
                // return this.dataToFilter.slice(0, value)
            }
        }
    }
</script>
