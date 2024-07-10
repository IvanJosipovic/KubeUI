using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using k8s;
using k8s.Models;

namespace KubeUI.Controls;

public partial class PodContainerCell : UserControl
{
    public PodContainerCell()
    {
        InitializeComponent();
    }

    public IList<V1Container> Containers
    {
        get
        {
            return GetContainers();
        }
    }

    private IList<V1Container> GetContainers()
    {
        if (DataContext is V1Pod pod)
        {
            return pod.Spec.Containers;
        }
        else
        {
            return [];
        }
    }
}

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool.TryParse(parameter?.ToString(), out var isInit);

        if (value is V1ContainerStatus status)
        {
            if (status.Ready == true && status.Started == true)
            {
                return Brushes.LimeGreen;
            }
            if (status.Ready == false && status.Started == true)
            {
                return Brushes.Orange;
            }
            else if (status.State.Waiting != null)
            {
                return Brushes.Orange;
            }
            else if (status.State.Terminated != null)
            {
                if (status.State.Terminated.Reason == "Completed")
                {
                    return Brushes.Gray;
                }

                return Brushes.Orange;
            }
        }

        return Brushes.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal partial class DemoPodContainerCell: V1Pod
{
    public DemoPodContainerCell()
    {
        var yaml = @"
apiVersion: v1
kind: Pod
metadata:
  name: nvidia-operator-validator-nqk2k
  generateName: nvidia-operator-validator-
  namespace: gpu-operator
  uid: 5a98b9b6-a8d7-410b-b665-7b4f8961a8c9
  resourceVersion: '387977711'
  creationTimestamp: '2023-06-08T22:49:39Z'
  labels:
    app: nvidia-operator-validator
    app.kubernetes.io/part-of: gpu-operator
    controller-revision-hash: 64799f456
    pod-template-generation: '6'
  ownerReferences:
    - apiVersion: apps/v1
      kind: DaemonSet
      name: nvidia-operator-validator
      uid: ab75b1e7-c75c-4c5c-a433-956c2408cea7
      controller: true
      blockOwnerDeletion: true
  managedFields:
    - manager: kubelite
      operation: Update
      apiVersion: v1
      time: '2023-06-08T22:49:39Z'
      fieldsType: FieldsV1
      fieldsV1:
        f:metadata:
          f:generateName: {}
          f:labels:
            .: {}
            f:app: {}
            f:app.kubernetes.io/part-of: {}
            f:controller-revision-hash: {}
            f:pod-template-generation: {}
          f:ownerReferences:
            .: {}
            k:{""uid"":""ab75b1e7-c75c-4c5c-a433-956c2408cea7""}: {}
        f:spec:
          f:affinity:
            .: {}
            f:nodeAffinity:
              .: {}
              f:requiredDuringSchedulingIgnoredDuringExecution: {}
          f:containers:
            k:{""name"":""nvidia-operator-validator""}:
              .: {}
              f:args: {}
              f:command: {}
              f:image: {}
              f:imagePullPolicy: {}
              f:lifecycle:
                .: {}
                f:preStop:
                  .: {}
                  f:exec:
                    .: {}
                    f:command: {}
              f:name: {}
              f:resources: {}
              f:securityContext:
                .: {}
                f:privileged: {}
              f:terminationMessagePath: {}
              f:terminationMessagePolicy: {}
              f:volumeMounts:
                .: {}
                k:{""mountPath"":""/run/nvidia/validations""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
          f:dnsPolicy: {}
          f:enableServiceLinks: {}
          f:initContainers:
            .: {}
            k:{""name"":""cuda-validation""}:
              .: {}
              f:args: {}
              f:command: {}
              f:env:
                .: {}
                k:{""name"":""COMPONENT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""NODE_NAME""}:
                  .: {}
                  f:name: {}
                  f:valueFrom:
                    .: {}
                    f:fieldRef: {}
                k:{""name"":""OPERATOR_NAMESPACE""}:
                  .: {}
                  f:name: {}
                  f:valueFrom:
                    .: {}
                    f:fieldRef: {}
                k:{""name"":""VALIDATOR_IMAGE""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""VALIDATOR_IMAGE_PULL_POLICY""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""VALIDATOR_RUNTIME_CLASS""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""WITH_WAIT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
              f:image: {}
              f:imagePullPolicy: {}
              f:name: {}
              f:resources: {}
              f:securityContext:
                .: {}
                f:privileged: {}
              f:terminationMessagePath: {}
              f:terminationMessagePolicy: {}
              f:volumeMounts:
                .: {}
                k:{""mountPath"":""/run/nvidia/validations""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
            k:{""name"":""driver-validation""}:
              .: {}
              f:args: {}
              f:command: {}
              f:env:
                .: {}
                k:{""name"":""COMPONENT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""WITH_WAIT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
              f:image: {}
              f:imagePullPolicy: {}
              f:name: {}
              f:resources: {}
              f:securityContext:
                .: {}
                f:privileged: {}
                f:seLinuxOptions:
                  .: {}
                  f:level: {}
              f:terminationMessagePath: {}
              f:terminationMessagePolicy: {}
              f:volumeMounts:
                .: {}
                k:{""mountPath"":""/host""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
                  f:readOnly: {}
                k:{""mountPath"":""/host-dev-char""}:
                  .: {}
                  f:mountPath: {}
                  f:name: {}
                k:{""mountPath"":""/run/nvidia/driver""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
                k:{""mountPath"":""/run/nvidia/validations""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
            k:{""name"":""plugin-validation""}:
              .: {}
              f:args: {}
              f:command: {}
              f:env:
                .: {}
                k:{""name"":""COMPONENT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""MIG_STRATEGY""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""NODE_NAME""}:
                  .: {}
                  f:name: {}
                  f:valueFrom:
                    .: {}
                    f:fieldRef: {}
                k:{""name"":""OPERATOR_NAMESPACE""}:
                  .: {}
                  f:name: {}
                  f:valueFrom:
                    .: {}
                    f:fieldRef: {}
                k:{""name"":""VALIDATOR_IMAGE""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""VALIDATOR_IMAGE_PULL_POLICY""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""VALIDATOR_RUNTIME_CLASS""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""WITH_WAIT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""WITH_WORKLOAD""}:
                  .: {}
                  f:name: {}
                  f:value: {}
              f:image: {}
              f:imagePullPolicy: {}
              f:name: {}
              f:resources: {}
              f:securityContext:
                .: {}
                f:privileged: {}
              f:terminationMessagePath: {}
              f:terminationMessagePolicy: {}
              f:volumeMounts:
                .: {}
                k:{""mountPath"":""/run/nvidia/validations""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
            k:{""name"":""toolkit-validation""}:
              .: {}
              f:args: {}
              f:command: {}
              f:env:
                .: {}
                k:{""name"":""COMPONENT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""NVIDIA_VISIBLE_DEVICES""}:
                  .: {}
                  f:name: {}
                  f:value: {}
                k:{""name"":""WITH_WAIT""}:
                  .: {}
                  f:name: {}
                  f:value: {}
              f:image: {}
              f:imagePullPolicy: {}
              f:name: {}
              f:resources: {}
              f:securityContext:
                .: {}
                f:privileged: {}
              f:terminationMessagePath: {}
              f:terminationMessagePolicy: {}
              f:volumeMounts:
                .: {}
                k:{""mountPath"":""/run/nvidia/validations""}:
                  .: {}
                  f:mountPath: {}
                  f:mountPropagation: {}
                  f:name: {}
          f:nodeSelector: {}
          f:priorityClassName: {}
          f:restartPolicy: {}
          f:runtimeClassName: {}
          f:schedulerName: {}
          f:securityContext: {}
          f:serviceAccount: {}
          f:serviceAccountName: {}
          f:terminationGracePeriodSeconds: {}
          f:tolerations: {}
          f:volumes:
            .: {}
            k:{""name"":""driver-install-path""}:
              .: {}
              f:hostPath:
                .: {}
                f:path: {}
                f:type: {}
              f:name: {}
            k:{""name"":""host-dev-char""}:
              .: {}
              f:hostPath:
                .: {}
                f:path: {}
                f:type: {}
              f:name: {}
            k:{""name"":""host-root""}:
              .: {}
              f:hostPath:
                .: {}
                f:path: {}
                f:type: {}
              f:name: {}
            k:{""name"":""run-nvidia-validations""}:
              .: {}
              f:hostPath:
                .: {}
                f:path: {}
                f:type: {}
              f:name: {}
    - manager: kubelite
      operation: Update
      apiVersion: v1
      time: '2024-04-02T08:57:58Z'
      fieldsType: FieldsV1
      fieldsV1:
        f:status:
          f:conditions:
            k:{""type"":""ContainersReady""}:
              .: {}
              f:lastProbeTime: {}
              f:lastTransitionTime: {}
              f:status: {}
              f:type: {}
            k:{""type"":""Initialized""}:
              .: {}
              f:lastProbeTime: {}
              f:lastTransitionTime: {}
              f:status: {}
              f:type: {}
            k:{""type"":""PodReadyToStartContainers""}:
              .: {}
              f:lastProbeTime: {}
              f:lastTransitionTime: {}
              f:status: {}
              f:type: {}
            k:{""type"":""Ready""}:
              .: {}
              f:lastProbeTime: {}
              f:lastTransitionTime: {}
              f:status: {}
              f:type: {}
          f:containerStatuses: {}
          f:hostIP: {}
          f:hostIPs: {}
          f:initContainerStatuses: {}
          f:phase: {}
          f:podIP: {}
          f:podIPs:
            .: {}
            k:{""ip"":""10.1.96.82""}:
              .: {}
              f:ip: {}
          f:startTime: {}
      subresource: status
  selfLink: /api/v1/namespaces/gpu-operator/pods/nvidia-operator-validator-nqk2k
status:
  phase: Running
  conditions:
    - type: PodReadyToStartContainers
      status: 'True'
      lastProbeTime: null
      lastTransitionTime: '2024-04-02T08:57:32Z'
    - type: Initialized
      status: 'True'
      lastProbeTime: null
      lastTransitionTime: '2024-01-18T21:01:02Z'
    - type: Ready
      status: 'True'
      lastProbeTime: null
      lastTransitionTime: '2024-04-02T08:57:58Z'
    - type: ContainersReady
      status: 'True'
      lastProbeTime: null
      lastTransitionTime: '2024-04-02T08:57:58Z'
    - type: PodScheduled
      status: 'True'
      lastProbeTime: null
      lastTransitionTime: '2023-06-08T22:49:39Z'
  hostIP: 192.168.1.120
  hostIPs:
    - ip: 192.168.1.120
  podIP: 10.1.96.82
  podIPs:
    - ip: 10.1.96.82
  startTime: '2023-06-08T22:49:39Z'
  initContainerStatuses:
    - name: driver-validation
      state:
        terminated:
          exitCode: 0
          reason: Completed
          startedAt: '2024-04-02T08:57:31Z'
          finishedAt: '2024-04-02T08:57:32Z'
          containerID: >-
            containerd://008e1f235b82690f21c1f8284730da64e749610db1b4169b95283d3b06f63be3
      lastState: {}
      ready: true
      restartCount: 7
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      imageID: >-
        nvcr.io/nvidia/cloud-native/gpu-operator-validator@sha256:21dfc9c56b5f8bce73e60361d6e83759c3fa14dc6afc2d5ebdf1b891a936daf6
      containerID: >-
        containerd://008e1f235b82690f21c1f8284730da64e749610db1b4169b95283d3b06f63be3
      started: false
    - name: toolkit-validation
      state:
        terminated:
          exitCode: 0
          reason: Completed
          startedAt: '2024-04-02T08:57:34Z'
          finishedAt: '2024-04-02T08:57:34Z'
          containerID: >-
            containerd://5db2743c9bd8f4f5425febb39f9c8befca66bd2953f54d7e8dfd46556d568153
      lastState: {}
      ready: true
      restartCount: 0
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      imageID: >-
        nvcr.io/nvidia/cloud-native/gpu-operator-validator@sha256:21dfc9c56b5f8bce73e60361d6e83759c3fa14dc6afc2d5ebdf1b891a936daf6
      containerID: >-
        containerd://5db2743c9bd8f4f5425febb39f9c8befca66bd2953f54d7e8dfd46556d568153
      started: false
    - name: cuda-validation
      state:
        terminated:
          exitCode: 0
          reason: Completed
          startedAt: '2024-04-02T08:57:35Z'
          finishedAt: '2024-04-02T08:57:56Z'
          containerID: >-
            containerd://9b7c684d619d7813cec9d5f2ef7249bb5d2cc7a4de23b1e27afc861d79b317c8
      lastState: {}
      ready: true
      restartCount: 0
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      imageID: >-
        nvcr.io/nvidia/cloud-native/gpu-operator-validator@sha256:21dfc9c56b5f8bce73e60361d6e83759c3fa14dc6afc2d5ebdf1b891a936daf6
      containerID: >-
        containerd://9b7c684d619d7813cec9d5f2ef7249bb5d2cc7a4de23b1e27afc861d79b317c8
      started: false
    - name: plugin-validation
      state:
        terminated:
          exitCode: 0
          reason: Completed
          startedAt: '2024-04-02T08:57:57Z'
          finishedAt: '2024-04-02T08:57:57Z'
          containerID: >-
            containerd://420c05e7cdafa98aeffd2af318c632a2c1f1057c9950eb1d2a5ed699c3ee08f8
      lastState: {}
      ready: true
      restartCount: 0
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      imageID: >-
        nvcr.io/nvidia/cloud-native/gpu-operator-validator@sha256:21dfc9c56b5f8bce73e60361d6e83759c3fa14dc6afc2d5ebdf1b891a936daf6
      containerID: >-
        containerd://420c05e7cdafa98aeffd2af318c632a2c1f1057c9950eb1d2a5ed699c3ee08f8
      started: false
  containerStatuses:
    - name: nvidia-operator-validator
      state:
        running:
          startedAt: '2024-04-02T08:57:58Z'
      lastState:
        terminated:
          exitCode: 255
          reason: Unknown
          startedAt: '2024-03-02T20:22:49Z'
          finishedAt: '2024-04-02T08:56:32Z'
          containerID: >-
            containerd://193131bce1b7df6bb5a91aba1a646973104b543e4e2d7c945e34db790abbbb72
      ready: true
      restartCount: 16
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      imageID: >-
        nvcr.io/nvidia/cloud-native/gpu-operator-validator@sha256:21dfc9c56b5f8bce73e60361d6e83759c3fa14dc6afc2d5ebdf1b891a936daf6
      containerID: >-
        containerd://75780ad9f4c46ef7ca0c0412ea0a2b5507b09990d42f47e34e2d7147cda0ebec
      started: true
  qosClass: BestEffort
spec:
  volumes:
    - name: run-nvidia-validations
      hostPath:
        path: /run/nvidia/validations
        type: DirectoryOrCreate
    - name: driver-install-path
      hostPath:
        path: /run/nvidia/driver
        type: ''
    - name: host-root
      hostPath:
        path: /
        type: ''
    - name: host-dev-char
      hostPath:
        path: /dev/char
        type: ''
    - name: kube-api-access-rj2gb
      projected:
        sources:
          - serviceAccountToken:
              expirationSeconds: 3607
              path: token
          - configMap:
              name: kube-root-ca.crt
              items:
                - key: ca.crt
                  path: ca.crt
          - downwardAPI:
              items:
                - path: namespace
                  fieldRef:
                    apiVersion: v1
                    fieldPath: metadata.namespace
        defaultMode: 420
  initContainers:
    - name: driver-validation
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      command:
        - sh
        - '-c'
      args:
        - nvidia-validator
      env:
        - name: WITH_WAIT
          value: 'true'
        - name: COMPONENT
          value: driver
      resources: {}
      volumeMounts:
        - name: host-root
          readOnly: true
          mountPath: /host
          mountPropagation: HostToContainer
        - name: driver-install-path
          mountPath: /run/nvidia/driver
          mountPropagation: HostToContainer
        - name: run-nvidia-validations
          mountPath: /run/nvidia/validations
          mountPropagation: Bidirectional
        - name: host-dev-char
          mountPath: /host-dev-char
        - name: kube-api-access-rj2gb
          readOnly: true
          mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      terminationMessagePath: /dev/termination-log
      terminationMessagePolicy: File
      imagePullPolicy: IfNotPresent
      securityContext:
        privileged: true
        seLinuxOptions:
          level: s0
    - name: toolkit-validation
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      command:
        - sh
        - '-c'
      args:
        - nvidia-validator
      env:
        - name: NVIDIA_VISIBLE_DEVICES
          value: all
        - name: WITH_WAIT
          value: 'false'
        - name: COMPONENT
          value: toolkit
      resources: {}
      volumeMounts:
        - name: run-nvidia-validations
          mountPath: /run/nvidia/validations
          mountPropagation: Bidirectional
        - name: kube-api-access-rj2gb
          readOnly: true
          mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      terminationMessagePath: /dev/termination-log
      terminationMessagePolicy: File
      imagePullPolicy: IfNotPresent
      securityContext:
        privileged: true
    - name: cuda-validation
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      command:
        - sh
        - '-c'
      args:
        - nvidia-validator
      env:
        - name: WITH_WAIT
          value: 'false'
        - name: COMPONENT
          value: cuda
        - name: NODE_NAME
          valueFrom:
            fieldRef:
              apiVersion: v1
              fieldPath: spec.nodeName
        - name: OPERATOR_NAMESPACE
          valueFrom:
            fieldRef:
              apiVersion: v1
              fieldPath: metadata.namespace
        - name: VALIDATOR_IMAGE
          value: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
        - name: VALIDATOR_IMAGE_PULL_POLICY
          value: IfNotPresent
        - name: VALIDATOR_RUNTIME_CLASS
          value: nvidia
      resources: {}
      volumeMounts:
        - name: run-nvidia-validations
          mountPath: /run/nvidia/validations
          mountPropagation: Bidirectional
        - name: kube-api-access-rj2gb
          readOnly: true
          mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      terminationMessagePath: /dev/termination-log
      terminationMessagePolicy: File
      imagePullPolicy: IfNotPresent
      securityContext:
        privileged: true
    - name: plugin-validation
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      command:
        - sh
        - '-c'
      args:
        - nvidia-validator
      env:
        - name: COMPONENT
          value: plugin
        - name: WITH_WAIT
          value: 'false'
        - name: WITH_WORKLOAD
          value: 'false'
        - name: MIG_STRATEGY
          value: single
        - name: NODE_NAME
          valueFrom:
            fieldRef:
              apiVersion: v1
              fieldPath: spec.nodeName
        - name: OPERATOR_NAMESPACE
          valueFrom:
            fieldRef:
              apiVersion: v1
              fieldPath: metadata.namespace
        - name: VALIDATOR_IMAGE
          value: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
        - name: VALIDATOR_IMAGE_PULL_POLICY
          value: IfNotPresent
        - name: VALIDATOR_RUNTIME_CLASS
          value: nvidia
      resources: {}
      volumeMounts:
        - name: run-nvidia-validations
          mountPath: /run/nvidia/validations
          mountPropagation: Bidirectional
        - name: kube-api-access-rj2gb
          readOnly: true
          mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      terminationMessagePath: /dev/termination-log
      terminationMessagePolicy: File
      imagePullPolicy: IfNotPresent
      securityContext:
        privileged: true
  containers:
    - name: nvidia-operator-validator
      image: nvcr.io/nvidia/cloud-native/gpu-operator-validator:v23.3.2
      command:
        - sh
        - '-c'
      args:
        - echo all validations are successful; sleep infinity
      resources: {}
      volumeMounts:
        - name: run-nvidia-validations
          mountPath: /run/nvidia/validations
          mountPropagation: Bidirectional
        - name: kube-api-access-rj2gb
          readOnly: true
          mountPath: /var/run/secrets/kubernetes.io/serviceaccount
      lifecycle:
        preStop:
          exec:
            command:
              - /bin/sh
              - '-c'
              - rm -f /run/nvidia/validations/*-ready
      terminationMessagePath: /dev/termination-log
      terminationMessagePolicy: File
      imagePullPolicy: IfNotPresent
      securityContext:
        privileged: true
  restartPolicy: Always
  terminationGracePeriodSeconds: 30
  dnsPolicy: ClusterFirst
  nodeSelector:
    nvidia.com/gpu.deploy.operator-validator: 'true'
  serviceAccountName: nvidia-operator-validator
  serviceAccount: nvidia-operator-validator
  nodeName: r720
  securityContext: {}
  affinity:
    nodeAffinity:
      requiredDuringSchedulingIgnoredDuringExecution:
        nodeSelectorTerms:
          - matchFields:
              - key: metadata.name
                operator: In
                values:
                  - r720
  schedulerName: default-scheduler
  tolerations:
    - key: nvidia.com/gpu
      operator: Exists
      effect: NoSchedule
    - key: node.kubernetes.io/not-ready
      operator: Exists
      effect: NoExecute
    - key: node.kubernetes.io/unreachable
      operator: Exists
      effect: NoExecute
    - key: node.kubernetes.io/disk-pressure
      operator: Exists
      effect: NoSchedule
    - key: node.kubernetes.io/memory-pressure
      operator: Exists
      effect: NoSchedule
    - key: node.kubernetes.io/pid-pressure
      operator: Exists
      effect: NoSchedule
    - key: node.kubernetes.io/unschedulable
      operator: Exists
      effect: NoSchedule
  priorityClassName: system-node-critical
  priority: 2000001000
  runtimeClassName: nvidia
  enableServiceLinks: true
  preemptionPolicy: PreemptLowerPriority

";

        var pod = KubernetesYaml.Deserialize<V1Pod>(yaml);
        this.Metadata = pod.Metadata;
        this.Spec = pod.Spec;
        this.Status = pod.Status;
    }
}
