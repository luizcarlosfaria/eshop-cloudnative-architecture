pipeline {
    
    agent none

    environment {
        COMPOSE_PROJECT_NAME = "${env.JOB_NAME}-${env.BUILD_ID}"
    }

    /*options {
        throttleJobProperty (
          categories: ['eshop'],
          throttleEnabled: true,
          throttleOption: 'category'
      )
    }*/

    stages {
      
        stage('Build') {

            agent {
                dockerfile {
                    args '-u root:root -v /gago/nuget-cache:/root/.nuget/packages'
                }
            }

            steps {
                
                echo sh(script: 'env|sort', returnStdout: true)

                sh 'dotnet build ./eShopCloudNative.Architecture.sln'

            }

        }
        

        stage('Test') {

            agent {
                dockerfile {
                    args '-u root:root -v /var/run/docker.sock:/var/run/docker.sock -v /gago/nuget-cache:/root/.nuget/packages '
                }
            }

            steps {

                 withCredentials([usernamePassword(credentialsId: 'SonarQube', passwordVariable: 'SONARQUBE_KEY', usernameVariable: 'SONARQUBE_USERNAME' )]) {

                    sh  '''
                       
                        dotnet test ./eShopCloudNative.Architecture.Tests/eShopCloudNative.Architecture.Tests.csproj \
                            --configuration Debug \
                            --output /output-tests  \
                            /p:CollectCoverage=true \
                            /p:CoverletOutputFormat=opencover \
                            /p:CoverletOutput='/output-coverage/coverage.xml' \
                            /p:Exclude="[Oragon.*.Tests]*"

                        dotnet sonarscanner begin \
                            /o:luizcarlosfaria \
                            /k:luizcarlosfaria_eshop-cloudnative-architecture \
                            /d:sonar.login="$SONARQUBE_KEY" \
                            /d:sonar.host.url="https://sonarcloud.io" \
                            /d:sonar.cs.opencover.reportsPaths="/output-coverage/coverage.xml"
                            
                        
                        dotnet build ./eShopCloudNative.Architecture.sln

                        dotnet sonarscanner end /d:sonar.login="$SONARQUBE_KEY"

                        '''

                }
                
            }

        }

        stage('Pack and Publish') {

            agent {
                dockerfile {
                    args '-u root:root -v /gago/nuget-cache:/root/.nuget/packages '
                }
            }

            when { buildingTag() }

            steps {

                script{

                    def projetcs = [
						'./eShopCloudNative.Architecture/eShopCloudNative.Architecture.csproj',
                        './eShopCloudNative.Architecture.Bootstrap/eShopCloudNative.Architecture.Bootstrap.csproj',
                        './eShopCloudNative.Architecture.Data/eShopCloudNative.Architecture.Data.csproj',
                        './eShopCloudNative.Architecture.WebApi/eShopCloudNative.Architecture.WebApi.csproj',
                        './eShopCloudNative.Architecture.Messaging/eShopCloudNative.Architecture.Messaging.csproj'
                    ]
                    

                    if (env.BRANCH_NAME.endsWith("-alpha")) {

                        for (int i = 0; i < projetcs.size(); ++i) {
                            sh "dotnet pack ${projetcs[i]} --configuration Debug /p:PackageVersion=${BRANCH_NAME} -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg --output ./output-packages"
                        }

                    } else if (env.BRANCH_NAME.endsWith("-beta")) {

                        for (int i = 0; i < projetcs.size(); ++i) {
                            sh "dotnet pack ${projetcs[i]} --configuration Release /p:PackageVersion=${BRANCH_NAME} -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg --output ./output-packages"                        
                        }

                    } else {

                        for (int i = 0; i < projetcs.size(); ++i) {
                            sh "dotnet pack ${projetcs[i]} --configuration Release /p:PackageVersion=${BRANCH_NAME} -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg --output ./output-packages"                        
                        }

                    }

                    
                    def publishOnNuGet = ( env.BRANCH_NAME.endsWith("-alpha") == false );
                        
                    withCredentials([usernamePassword(credentialsId: 'myget-eshop-cloud-native', passwordVariable: 'MYGET_KEY', usernameVariable: 'DUMMY' )]) {

                        sh 'for pkg in ./output-packages/*.nupkg ; do dotnet nuget push "$pkg" -k "$MYGET_KEY" -s https://www.myget.org/F/eshop-cloud-native/api/v3/index.json ; done'

                        sh 'for pkg in ./output-packages/*.snupkg ; do dotnet nuget push "$pkg" -k "$MYGET_KEY" -s https://www.myget.org/F/eshop-cloud-native/api/v3/index.json ; done'
						
                    }

                    if (publishOnNuGet) {

                        withCredentials([usernamePassword(credentialsId: 'nuget-luizcarlosfaria', passwordVariable: 'NUGET_KEY', usernameVariable: 'DUMMY')]) {

                            sh 'for pkg in ./output-packages/*.nupkg ; do dotnet nuget push "$pkg" -k "$NUGET_KEY" -s https://api.nuget.org/v3/index.json ; done'

                        }

                    }                    
				}
            }
        }
    }
    /*post {

        always {
            node('master'){
                
                sh  '''
                
                #docker-compose down -v

                '''


            }
        }
    }*/
}