<template>
    <div class="col-lg-2 text-center">
        <div id="show-env" :class="[{ 'hide' : hideBanner }, currentEnvColor]">
            {{currentEnvironment}}
        </div>
    </div>
</template>

<script>
    import {environment} from "../utils/constants";
    export default {
        name: "EnvBanner",
        props: {},
        data (){
            return {
                hideBanner: false,
                currentEnvironment: "",
                currentEnvColor: ''
            }
        },
        created(){
            this.getEnvironment(environment)
        },
        methods: {
            getEnvironment(environment){
                switch (environment) {
                    case 'dev':
                        this.currentEnvironment = "Dev"
                        this.currentEnvColor = 'greenish'
                        break;
                    case 'test':
                        this.currentEnvironment = "Test"
                        this.currentEnvColor = 'greenish'
                        break;
                    case 'staging':
                        this.currentEnvironment = "Staging"
                        this.currentEnvColor = 'reddish'
                        break;
                    case 'Demo':
                        this.currentEnvironment = "Demo"
                        this.currentEnvColor = 'transparent'
                        break;
                    default:
                        this.hideBanner = true
                        break;
                }
            }
        }
    }
</script>

<style scoped>
    #show-env {
        text-align: center;
        text-transform: uppercase;
        font-family: arial, sans-serif, 'Open Sans';
        margin-top: 1.3rem;
        padding: 7px;
        width: 6rem;
    }
    #slide-out {
        overflow-x: hidden;
    }
    #show-env.greenish {
        background-color: #38bf0e;
        color: #fff;
    }
    #show-env.reddish {
        background-color: #F44336;
        color: #fff;
    }

    #show-env.transparent {
        background-color: transparent;
    }
</style>
