<template>
    <div class="footer">
        <div class="row">
        <div class="col-lg-9 text-center">
            <small> BoU Auth UI Version: {{emataUIVersion}}</small>
            <!-- <small class="ml-2"> Emata Auth API Version: {{emataUIVersion}}</small> -->
        </div>
        <div class="col-lg-3 text-right">
            <small>Product of Bank of Uganda &copy; 2021</small>
        </div>
        </div>
    </div>
</template>

<script>
    import {uiVersion} from "../utils/constants";
    export default {
        name: "Footer",
        computed: {
            emataUIVersion: function () {
                return uiVersion 
            }
        }
    }
</script>
